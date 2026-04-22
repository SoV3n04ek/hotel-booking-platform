using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using HotelBooking.Application.DTOs.Bookings;
using HotelBooking.Application.Interfaces;
using HotelBooking.Application.Services;
using HotelBooking.Application.Validators;
using HotelBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using System.Linq.Expressions;
using HotelBooking.Application.Tests.Infrastructure.EfHelpers;
using Xunit;

namespace HotelBooking.Application.Tests.Services;

public class BookingServiceTest
{
    private readonly Mock<IBookingRepository> _bookingRepo = new();
    private readonly Mock<IRoomRepository> _roomRepo = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly IValidator<CreateBookingRequest> _validator;
    private readonly BookingService _service;
    private readonly DateTimeOffset _baseDate = new(2026, 6, 1, 12, 0, 0, TimeSpan.Zero);

    public BookingServiceTest()
    {
        _validator = new CreateBookingRequestValidator();
        _roomRepo.Setup(r => r.GetAll()).Returns(new TestAsyncEnumerable<Room>(new List<Room>()));

        _service = new BookingService(_bookingRepo.Object, _roomRepo.Object, _uow.Object, _validator);
    }

    [Fact]
    public async Task CreateBooking_ShouldThrowValidationException_WhenDatesAreInvalid()
    {
        var request = new CreateBookingRequest(1, 1, _baseDate.AddDays(5), _baseDate.AddDays(2));
        
        var act = async () => await _service.CreateBookingAsync(request);

        var exception = await act.Should().ThrowAsync<ValidationException>();

        exception.Which.Errors.Should().Contain(e =>
            e.PropertyName == "CheckOut" &&
            e.ErrorMessage.Contains("later than the check-in"));
    }

    [Fact]
    public async Task CreateBooking_ShouldSucceed_WhenDataIsValid()
    {
        var room = new Room { Id = 1, PricePerNight = 100, Version = 1 };
        var request = new CreateBookingRequest(1, 1, _baseDate.AddDays(1), _baseDate.AddDays(3));

        _roomRepo.Setup(r => r.GetRoomIfAvailableAsync(It.IsAny<int>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(room);

        var result = await _service.CreateBookingAsync(request);

        result.Should().NotBeNull();
        _bookingRepo.Verify(repo => repo.AddAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _roomRepo.Verify(repo => repo.Update(It.Is<Room>(r => r.Id == room.Id)), Times.Once);
    }

    [Fact]
    public async Task CreateBooking_ShouldThrowNotFound_WhenRoomDoesNotExist()
    {
        var request = new CreateBookingRequest(1, 999, _baseDate.AddDays(1), _baseDate.AddDays(2));

        _roomRepo.Setup(r => r.GetRoomIfAvailableAsync(It.IsAny<int>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Room?)null);

        var act = async () => await _service.CreateBookingAsync(request);

        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("Room not found.");
    }

    [Fact]
    public async Task CreateBooking_ShouldThrowConflict_WhenRoomIsOccupied()
    {
        var request = new CreateBookingRequest(1, 1, _baseDate.AddDays(1), _baseDate.AddDays(2));

        _roomRepo.Setup(r => r.GetRoomIfAvailableAsync(It.IsAny<int>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Room?)null);

        _roomRepo.Setup(r => r.GetAll()).Returns(new TestAsyncEnumerable<Room>(new List<Room> { new() { Id = 1 } }));

        var act = async () => await _service.CreateBookingAsync(request);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Room is already occupied for these dates.");
    }

    [Fact]
    public async Task CreateBooking_ShouldThrowConflict_WhenDbUpdateConcurrencyExceptionOccurs()
    {
        var request = new CreateBookingRequest(1, 1, _baseDate.AddDays(1), _baseDate.AddDays(2));
        _roomRepo.Setup(r => r.GetRoomIfAvailableAsync(It.IsAny<int>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Room { Id = 1, PricePerNight = 100 });

        _uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new DbUpdateConcurrencyException());

        var act = async () => await _service.CreateBookingAsync(request);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("This room was just booked by someone else. Please refresh.");
    }

    [Fact]
    public async Task CreateBooking_ShouldCalculateCorrectPrice_ForMultiNightStay()
    {
        var request = new CreateBookingRequest(1, 1, _baseDate, _baseDate.AddDays(3));
        _roomRepo.Setup(r => r.GetRoomIfAvailableAsync(It.IsAny<int>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Room { Id = 1, PricePerNight = 150.50m });

        var result = await _service.CreateBookingAsync(request);

        result.TotalPrice.Should().Be(451.50m);
    }

    [Fact]
    public async Task CreateBooking_ShouldStopExecution_WhenCancellationTokenIsTriggered()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel(); // Trigger cancellation immediately

        var request = new CreateBookingRequest(1, 1, _baseDate, _baseDate.AddDays(1));

        // Act
        var act = async () => await _service.CreateBookingAsync(request, cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();

        // Verify the database was never touched
        _bookingRepo.Verify(r => r.AddAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetBookingById_ShouldReturnResponse_WhenBookingExists()
    {
        _bookingRepo.Setup(r => r.GetByIdAsync(42, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Booking { Id = 42 });

        var result = await _service.GetBookingByIdAsync(42);

        result.Should().NotBeNull();
        result!.Id.Should().Be(42);
    }
}
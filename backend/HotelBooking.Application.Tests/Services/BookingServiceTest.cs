using FluentAssertions;
using FluentValidation;
using HotelBooking.Application.DTOs.Bookings;
using HotelBooking.Application.Interfaces;
using HotelBooking.Application.Services;
using HotelBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace HotelBooking.Application.Tests.Services;

public class BookingServiceTest
{
    private readonly Mock<IBookingRepository> _bookingRepo;
    private readonly Mock<IRoomRepository> _roomRepo;
    private readonly Mock<IUnitOfWork> _uow;
    private readonly Mock<IValidator<CreateBookingRequest>> _validator;
    private readonly BookingService _service;
    private readonly DateTimeOffset _baseDate = new DateTimeOffset(2026, 6, 1, 12, 0, 0, TimeSpan.Zero);

    public BookingServiceTest()
    {
        _bookingRepo = new Mock<IBookingRepository>();
        _roomRepo = new Mock<IRoomRepository>();
        _uow = new Mock<IUnitOfWork>();
        _validator = new Mock<IValidator<CreateBookingRequest>>();

        _service = new BookingService(
            _bookingRepo.Object,
            _roomRepo.Object,
            _uow.Object,
            _validator.Object);
    }

    [Fact]
    public async Task CreateBooking_ShouldThrowException_WhenDatesAreInvalid()
    {
        // Arrange
        var request = new CreateBookingRequest(
            1, 1,
            _baseDate.AddDays(5),
            _baseDate.AddDays(2) // Checkout before Check-in
        );

        _roomRepo.Setup(repo => repo.GetRoomIfAvailableAsync(
                It.IsAny<int>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Room { Id = 1, PricePerNight = 100 });

        // Act
        var act = async () => await _service.CreateBookingAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Minimum stay is 1 night.");
    }

    [Fact]
    public async Task CreateBooking_ShouldSucceed_WhenDataIsValid()
    {
        // Arrange
        var room = new Room { Id = 1, PricePerNight = 100, Version = 1 };
        var request = new CreateBookingRequest(
            1, 1,
            _baseDate.AddDays(1),
            _baseDate.AddDays(3));

        _roomRepo.Setup(r => r.GetRoomIfAvailableAsync(
                It.IsAny<int>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(room);

        // Act
        var result = await _service.CreateBookingAsync(request);

        // Assert
        result.Should().NotBeNull();

        // Ensure data was persisted
        _bookingRepo.Verify(repo => repo.AddAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        // Verify the "Touch" for concurrency protection
        _roomRepo.Verify(repo => repo.Update(It.Is<Room>(r => r.Id == room.Id)), Times.Once);
    }

    [Fact]
    public async Task CreateBooking_ShouldThrowNotFound_WhenRoomDoesNotExist()
    {
        // Arrange
        var request = new CreateBookingRequest(1, 999, _baseDate.AddDays(1), _baseDate.AddDays(2));

        // availability check returns null
        _roomRepo.Setup(r => r.GetRoomIfAvailableAsync(It.IsAny<int>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Room?)null);

        // secondary check (existence) also returns null
        _roomRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Room?)null);

        // Act
        var act = async () => await _service.CreateBookingAsync(request);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Room not found.");
    }

    [Fact]
    public async Task CreateBooking_ShouldThrowConflict_WhenRoomIsOccupied()
    {
        // Arrange
        var request = new CreateBookingRequest(1, 1, _baseDate.AddDays(1), _baseDate.AddDays(2));

        // availability check returns null (occupied)
        _roomRepo.Setup(r => r.GetRoomIfAvailableAsync(It.IsAny<int>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Room?)null);

        // secondary check confirms the room exists
        _roomRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Room { Id = 1 });
        
        // Act
        var act = async () => await _service.CreateBookingAsync(request);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Room is already occupied for these dates.");
    }
}
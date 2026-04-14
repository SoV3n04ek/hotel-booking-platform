using FluentAssertions;
using FluentValidation;
using HotelBooking.Application.DTOs.Bookings;
using HotelBooking.Application.Interfaces;
using HotelBooking.Application.Services;
using HotelBooking.Domain.Entities;
using Moq;

namespace HotelBooking.Application.Tests.Services;

public class BookingServiceTest
{
    [Fact]
    public async Task CreateBooking_ShouldThrowException_WhenDatesAreInvalid()
    {
        // Arrange 
        var mockBookingRepo = new Mock<IBookingRepository>();
        var mockRoomRepo = new Mock<IRoomRepository>();
        var mockUow = new Mock<IUnitOfWork>();
        var mockValidator = new Mock<IValidator<CreateBookingRequest>>();

        var service = new BookingService(
            mockBookingRepo.Object,
            mockRoomRepo.Object,
            mockUow.Object,
            mockValidator.Object);

        var request = new CreateBookingRequest(
            1, 1,
            DateTimeOffset.UtcNow.AddDays(5),
            DateTimeOffset.UtcNow.AddDays(2)
        );

        mockRoomRepo.Setup(repo => repo.IsRoomAvailableAsync(
            It.IsAny<int>(),
            It.IsAny<DateTimeOffset>(),
            It.IsAny<DateTimeOffset>(),
            It.IsAny<CancellationToken>())) // Explicit token
            .ReturnsAsync(true);

        mockRoomRepo.Setup(repo => repo.GetByIdAsync(
            It.IsAny<int>(),
            It.IsAny<CancellationToken>())) // Explicit token
            .ReturnsAsync(new Room { Id = 1, PricePerNight = 100 });

        // Act
        var act = async () => await service.CreateBookingAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Minimum stay is 1 night.");
    }

    [Fact]
    public async Task CreateBooking_ShouldSucceed_WhenDataIsValid()
    {
        // Arrange
        var mockBookingRepo = new Mock<IBookingRepository>();
        var mockRoomRepo = new Mock<IRoomRepository>();
        var mockUow = new Mock<IUnitOfWork>();
        var mockValidator = new Mock<IValidator<CreateBookingRequest>>();

        var service = new BookingService(mockBookingRepo.Object, mockRoomRepo.Object, mockUow.Object, mockValidator.Object);

        var room = new Room { Id = 1, PricePerNight = 100, Version = 1 };
        var request = new CreateBookingRequest(1, 1, DateTimeOffset.UtcNow.AddDays(1), DateTimeOffset.UtcNow.AddDays(3));

        mockRoomRepo.Setup(r => r.GetRoomIfAvailableAsync(It.IsAny<int>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(room);

        // Act
        var result = await service.CreateBookingAsync(request);

        // Assert
        result.Should().NotBeNull();
        mockBookingRepo.Verify(repo => repo.AddAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()), Times.Once);

        mockUow.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateBooking_ShouldThrowNotFound_WhenRoomDoesNotExist()
    {
        var mockRoomRepo = new Mock<IRoomRepository>();
        var service = new BookingService(
            new Mock<IBookingRepository>().Object,
            mockRoomRepo.Object,
            new Mock<IUnitOfWork>().Object,
            new Mock<IValidator<CreateBookingRequest>>().Object
        );

        var request = new CreateBookingRequest(1, 999, DateTimeOffset.UtcNow.AddDays(1), DateTimeOffset.UtcNow.AddDays(2));

        // Setup: Room not found in availability check
        mockRoomRepo.Setup(r => r.GetRoomIfAvailableAsync(It.IsAny<int>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Room?)null);

        // Act
        var act = async () => await service.CreateBookingAsync(request);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("Room not found.");
    }
}
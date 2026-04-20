using FluentAssertions;
using HotelBooking.Application.Interfaces;
using HotelBooking.Application.Services;
using HotelBooking.Domain.Entities;
using Moq;
using Xunit;

namespace HotelBooking.Application.Tests.Services;

public class RoomServiceTest
{
    private readonly Mock<IRoomRepository> _roomRepo = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly RoomService _service;

    public RoomServiceTest()
    {
        _service = new RoomService(_roomRepo.Object, _uow.Object);
    }

    // ==========================================
    // Tests for GetAvailableRoomsByHotelIdAsync
    // ==========================================

    [Fact]
    public async Task GetAvailableRoomsByHotelId_ShouldThrowArgumentException_WhenCheckInIsAfterCheckOut()
    {
        // Arrange
        var hotelId = 1;
        var checkIn = DateTimeOffset.UtcNow.AddDays(2);
        var checkOut = DateTimeOffset.UtcNow.AddDays(1); // Check-out is before Check-in

        // Act
        var act = async () => await _service.GetAvailableRoomsByHotelIdAsync(hotelId, checkIn, checkOut);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Check-in date must be earlier than check-out date.");
    }

    [Fact]
    public async Task GetAvailableRoomsByHotelId_ShouldThrowArgumentException_WhenCheckInIsInThePast()
    {
        // Arrange
        var hotelId = 1;
        var checkIn = DateTimeOffset.UtcNow.AddDays(-1); // In the past
        var checkOut = DateTimeOffset.UtcNow.AddDays(2);

        // Act
        var act = async () => await _service.GetAvailableRoomsByHotelIdAsync(hotelId, checkIn, checkOut);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Cannot check in for a date in the past.");
    }

    [Fact]
    public async Task GetAvailableRoomsByHotelId_ShouldReturnMappedRooms_WhenDatesAreValid()
    {
        // Arrange
        var hotelId = 1;
        var checkIn = DateTimeOffset.UtcNow.AddDays(1);
        var checkOut = DateTimeOffset.UtcNow.AddDays(3);

        var roomsInDb = new List<Room>
        {
            new() { Id = 101, HotelId = hotelId, PricePerNight = 150m, Capacity = 2 },
            new() { Id = 102, HotelId = hotelId, PricePerNight = 250m, Capacity = 4 }
        };

        _roomRepo.Setup(r => r.GetAvailableRoomsAsync(
                hotelId, checkIn, checkOut, It.IsAny<CancellationToken>()))
            .ReturnsAsync(roomsInDb);

        // Act
        var result = await _service.GetAvailableRoomsByHotelIdAsync(hotelId, checkIn, checkOut);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);

        // Verify Mapping
        var firstRoom = result.First();
        firstRoom.Id.Should().Be(101);
        firstRoom.HotelId.Should().Be(hotelId);
        firstRoom.PricePerNight.Should().Be(150m);
        firstRoom.Capacity.Should().Be(2);
        firstRoom.IsAvailable.Should().BeTrue(); // Hardcoded to true in the service map
    }

    // ==========================================
    // Tests for GetRoomByIdAsync
    // ==========================================

    [Fact]
    public async Task GetRoomByIdAsync_ShouldReturnNull_WhenRoomDoesNotExist()
    {
        // Arrange
        var roomId = 999;
        _roomRepo.Setup(r => r.GetByIdAsync(roomId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Room?)null);

        // Act
        var result = await _service.GetRoomByIdAsync(roomId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetRoomByIdAsync_ShouldReturnMappedRoomResponse_WhenRoomExists()
    {
        // Arrange
        var roomId = 1;
        var roomInDb = new Room
        {
            Id = roomId,
            HotelId = 2,
            PricePerNight = 300m,
            Capacity = 3
        };

        _roomRepo.Setup(r => r.GetByIdAsync(roomId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(roomInDb);

        // Act
        var result = await _service.GetRoomByIdAsync(roomId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(roomId);
        result.HotelId.Should().Be(2);
        result.PricePerNight.Should().Be(300m);
        result.Capacity.Should().Be(3);
        result.IsAvailable.Should().BeTrue();
    }
}
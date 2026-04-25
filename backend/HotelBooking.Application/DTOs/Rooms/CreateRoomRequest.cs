namespace HotelBooking.Application.DTOs.Rooms;

public record CreateRoomRequest(
    int HotelId,
    decimal PricePerNight,
    int Capacity);

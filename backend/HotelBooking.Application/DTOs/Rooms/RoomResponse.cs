namespace HotelBooking.Application.DTOs.Rooms;

public record RoomResponse(
    int Id,
    int HotelId,
    decimal PricePerNight,
    int Capacity,
    bool IsAvailable);

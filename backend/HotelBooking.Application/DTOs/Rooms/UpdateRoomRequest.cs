namespace HotelBooking.Application.DTOs.Rooms;

public record UpdateRoomRequest(
    decimal PricePerNight,
    int Capacity);

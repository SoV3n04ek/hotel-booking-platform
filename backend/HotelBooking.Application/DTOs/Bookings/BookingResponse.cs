namespace HotelBooking.Application.DTOs.Bookings;

public record BookingResponse(
    int Id,
    int RoomId,
    int UserId,
    DateTimeOffset CheckIn,
    DateTimeOffset CheckOut,
    decimal TotalPrice,
    string Status
);
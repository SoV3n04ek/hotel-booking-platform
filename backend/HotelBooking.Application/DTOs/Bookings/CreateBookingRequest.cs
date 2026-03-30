namespace HotelBooking.Application.DTOs.Bookings;

public record CreateBookingRequest (
    int UserId,
    int RoomId,
    DateTimeOffset CheckIn,
    DateTimeOffset CheckOut);
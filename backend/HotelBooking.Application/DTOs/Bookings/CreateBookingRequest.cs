namespace HotelBooking.Application.DTOs.Bookings;

public record CreateBookingRequest
{
    public int UserId;
    public int RoomId;
    public DateTimeOffset CheckIn;
    public DateTimeOffset CheckOut;
}

namespace HotelBooking.Application.DTOs.Bookings;

public class CreateBookingRequest
{
    public int UserId { get; set; }
    public int RoomId { get; set; }
    public DateTimeOffset CheckIn { get; set; }
    public DateTimeOffset CheckOut { get; set; }
}

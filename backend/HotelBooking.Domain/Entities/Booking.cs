using HotelBooking.Domain.Common;

namespace HotelBooking.Domain.Entities;

public class Booking : BaseEntity
{
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int RoomId { get; set; }
    public Room Room { get; set; } = null!;

    public DateTimeOffset DateCheckIn { get; set; }
    public DateTimeOffset DateCheckOut { get; set; }

    public decimal TotalPrice { get; set; }
    public BookingStatus Status { get; set; } = BookingStatus.Pending;

    public void CalculateTotalPrice(decimal priceAtBookingTime)
    {
        var days = (DateCheckOut - DateCheckIn).Days;
        if (days <= 0) days = 1;

        TotalPrice = priceAtBookingTime * days;
    }
}
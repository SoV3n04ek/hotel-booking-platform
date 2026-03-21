using HotelBooking.Domain.Common;

namespace HotelBooking.Domain.Entities;

public class Room : BaseEntity
{
    public decimal PricePerNight { get; set; }
    public int Capacity { get; set; }

    public int HotelId { get; set; }
    public Hotel Hotel { get; set; } = null!;

    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
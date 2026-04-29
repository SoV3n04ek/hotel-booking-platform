namespace HotelBooking.Domain.Entities;
public class RoomImage : BaseImage
{
    public int RoomId { get; set; }
    public Room Room { get; set; } = null!; // Navigation property
}
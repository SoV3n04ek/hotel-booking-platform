namespace HotelBooking.Domain.Entities
{
    public class HotelImage : BaseImage
    {
        public int HotelId { get; set; }
        public Hotel Hotel { get; set; } = null!; // Navigation property
    }
}

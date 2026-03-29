namespace HotelBooking.Application.DTOs.Hotels;

public record HotelResponse
{
    public int Id;
    public string Name;
    public string Address;
    public string Description;
    public int TotalRooms;
}

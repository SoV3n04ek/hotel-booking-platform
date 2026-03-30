namespace HotelBooking.Application.DTOs.Hotels;
public record CreateHotelRequest (
    string Name, 
    string Address, 
    string Description);
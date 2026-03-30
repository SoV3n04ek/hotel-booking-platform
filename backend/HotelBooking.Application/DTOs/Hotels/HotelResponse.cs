namespace HotelBooking.Application.DTOs.Hotels;

public record HotelResponse(
    int Id,
    string Name, 
    string Address, 
    string Description, 
    int TotalRooms);
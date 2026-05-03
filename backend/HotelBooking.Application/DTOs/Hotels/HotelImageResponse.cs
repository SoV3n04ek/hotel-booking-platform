namespace HotelBooking.Application.DTOs.Hotels;

public record HotelImageResponse(
    Guid Id,
    int HotelId,
    string Url,
    string AltText,
    bool IsPrimary,
    int DisplayOrder);
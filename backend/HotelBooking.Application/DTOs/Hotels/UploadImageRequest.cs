using Microsoft.AspNetCore.Http;

namespace HotelBooking.Application.DTOs.Hotels;

public record UploadImageRequest(
    IFormFile File,
    string AltText,
    bool IsPrimary);
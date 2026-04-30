using HotelBooking.Application.Interfaces;

namespace HotelBooking.Application.Services;

public class LocalFileService : IFileService
{
    // Save files to wwwroot/uploads/hotels/ or wwwroot/uploads/rooms/.

    // Generate unique file names using Guid.NewGuid() to avoid overwriting files with the same name.
}
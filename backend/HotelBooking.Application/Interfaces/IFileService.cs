using Microsoft.AspNetCore.Http;

namespace HotelBooking.Application.Interfaces;

public interface IFileService
{
    Task<string> SaveFileAsync(IFormFile file, string subFolder, CancellationToken ct = default);
    Task DeleteFileAsync(string fileUrl, CancellationToken ct = default);
}

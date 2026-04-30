using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;

using Microsoft.AspNetCore.Http.IFormFile;
using HotelBooking.Application.Interfaces;

namespace HotelBooking.Infrastructure.Services;

public class LocalFileService : IFileService
{
    private readonly IWebHostEnvironment _env;

    public LocalFileService(IWebHostEnvironment env)
    {
        _env = env;
    }

    public async Task<string> SaveFileAsync(IFormFile file, string subFolder, CancellationToken ct = default)
    {
        var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", subFolder);

        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var filePath = Path.Combine(uploadsFolder, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream, ct);
        }

        return Path.Combine("uploads", subFolder, fileName).Replace("\\", "/");
    }

    public Task DeleteFileAsync(string fileUrl, CancellationToken ct = default)
    {
        // Convert the relative URL back to a physical path
        var filePath = Path.Combine(_env.WebRootPath, fileUrl);

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        return Task.CompletedTask;
    }
}
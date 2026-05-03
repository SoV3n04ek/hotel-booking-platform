using HotelBooking.Application.Interfaces;
using HotelBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Infrastructure.Repositories;

public class HotelImageRepository : IHotelImageRepository
{
    private readonly HotelsDbContext _context;

    public HotelImageRepository(HotelsDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(HotelImage image, CancellationToken ct)
    {
        await _context.HotelImages.AddAsync(image, ct);
    }

    public async Task<HotelImage?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await _context.HotelImages.FirstOrDefaultAsync(i => i.Id == id, ct);
    }

    public async Task UnsetPrimaryImagesAsync(int hotelId, CancellationToken ct)
    {
        var primaries = await _context.HotelImages
            .Where(i => i.HotelId == hotelId && i.IsPrimary)
            .ToListAsync(ct);

        foreach (var img in primaries) img.IsPrimary = false;
    }

    public void Delete(HotelImage image)
    {
        _context.HotelImages.Remove(image);
    }
}
using HotelBooking.Application.Interfaces;
using HotelBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Infrastructure.Repositories;

public class RoomRepository : GenericRepository<Room>, IRoomRepository
{
    public RoomRepository(HotelsDbContext context) : base(context) { }

    public async Task<bool> IsRoomAvailableAsync(int roomId, DateTimeOffset start, DateTimeOffset end, CancellationToken ct = default)
    {
        return !await _context.Bookings.AnyAsync(b =>
            b.RoomId == roomId &&
            b.DateCheckIn < end &&
            b.DateCheckOut > start, ct);
    }

    public async Task<IEnumerable<Room>> GetAvailableRoomsAsync(int hotelId, DateTimeOffset start, DateTimeOffset end, CancellationToken ct = default)
    {
        return await _context.Rooms
            .Where(r => r.HotelId == hotelId)
            .Where(r => !r.Bookings.Any(b =>
                b.DateCheckIn < end && b.DateCheckOut > start))
            .ToListAsync(ct);
    }

    public async Task<Room?> GetRoomIfAvailableAsync(int id, DateTimeOffset start, DateTimeOffset end, CancellationToken ct)
    {
        return await _context.Rooms
            .Where(r => r.Id == id)
            .Where(r => !r.Bookings.Any(b =>
                b.DateCheckIn < end &&
                b.DateCheckOut > start))
            .FirstOrDefaultAsync(ct);
    }
}

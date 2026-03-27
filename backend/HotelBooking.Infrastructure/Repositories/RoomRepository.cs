using HotelBooking.Application.Interfaces;
using HotelBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Infrastructure.Repositories;

public class RoomRepository : GenericRepository<Room>, IRoomRepository
{
    public RoomRepository(HotelsDbContext context) : base(context) { }

    public async Task<bool> IsRoomAvailableAsync(int roomId, DateTimeOffset start, DateTimeOffset end)
    {
        return !await _context.Bookings.AnyAsync(b =>
            b.RoomId == roomId &&
            b.DateCheckIn < end &&
            b.DateCheckOut > start);
    }

    public async Task<IEnumerable<Room>> GetAvailableRoomsAsync(int hotelId, DateTimeOffset start, DateTimeOffset end)
    {
        return await _context.Rooms
            .Where(r => r.HotelId == hotelId)
            .Where(r => !r.Bookings.Any(b =>
                b.DateCheckIn < end && b.DateCheckOut > start))
            .ToListAsync();
    }
}

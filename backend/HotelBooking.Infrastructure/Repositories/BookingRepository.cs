using HotelBooking.Application.Interfaces;
using HotelBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Infrastructure.Repositories;

public class BookingRepository : GenericRepository<Booking>, IBookingRepository
{
    public BookingRepository(HotelsDbContext context) : base(context) { }

    public async Task<IEnumerable<Booking>> GetByUserIdAsync(int userId)
    {
        return await _dbSet
            .Where(b => b.UserId == userId)
            .ToListAsync();
    }

    public async Task<bool> IsRoomAvailableAsync(int roomId, DateTimeOffset checkIn, DateTimeOffset checkOut)
    {
        bool hasOverlap = await _dbSet.AnyAsync(b =>
            b.RoomId == roomId &&
            b.DateCheckIn < checkOut &&
            b.DateCheckOut > checkIn);

        return !hasOverlap;
    }
}

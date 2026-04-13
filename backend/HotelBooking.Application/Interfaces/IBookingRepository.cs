using HotelBooking.Domain.Entities;

namespace HotelBooking.Application.Interfaces;

public interface IBookingRepository : IGenericRepository<Booking>
{
    Task<IEnumerable<Booking>> GetByUserIdAsync(int userId, CancellationToken ct = default);
    Task<bool> IsRoomAvailableAsync(int roomId, DateTimeOffset checkIn, DateTimeOffset checkOut, CancellationToken ct = default);
}

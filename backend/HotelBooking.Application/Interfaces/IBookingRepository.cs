using HotelBooking.Domain.Entities;

namespace HotelBooking.Application.Interfaces;

public interface IBookingRepository : IGenericRepository<Booking>
{
    Task<IEnumerable<Booking>> GetByUserIdAsync(int userId);
    Task<bool> IsRoomAvailableAsync(int roomId, DateTimeOffset checkIn, DateTimeOffset checkOut);
}

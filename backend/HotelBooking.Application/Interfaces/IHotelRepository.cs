using HotelBooking.Domain.Entities;

namespace HotelBooking.Application.Interfaces;

public interface IHotelRepository : IGenericRepository<Hotel>
{
    Task<IEnumerable<Room>> GetAvailableRoomsAsync(int hotelId, DateTimeOffset start, DateTimeOffset end);
}
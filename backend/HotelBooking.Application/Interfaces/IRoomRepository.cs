using HotelBooking.Domain.Entities;

namespace HotelBooking.Application.Interfaces;

public interface IRoomRepository : IGenericRepository<Room>
{
    Task<Room?> GetRoomIfAvailableAsync(int id, DateTimeOffset start, DateTimeOffset end, CancellationToken ct);
    Task<bool> IsRoomAvailableAsync(int roomId, DateTimeOffset start, DateTimeOffset end, CancellationToken ct = default);
    Task<IEnumerable<Room>> GetAvailableRoomsAsync(int hotelId, DateTimeOffset start, DateTimeOffset end, CancellationToken ct = default);
}

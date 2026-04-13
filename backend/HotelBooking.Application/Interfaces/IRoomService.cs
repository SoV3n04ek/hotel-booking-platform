using HotelBooking.Application.DTOs.Rooms;

namespace HotelBooking.Application.Interfaces;

public interface IRoomService
{
    Task<IEnumerable<RoomResponse>> GetAvailableRoomsByHotelIdAsync(int hotelId, DateTimeOffset checkIn, DateTimeOffset checkOut, CancellationToken ct = default);
    Task<RoomResponse?> GetRoomByIdAsync(int id, CancellationToken ct = default);
}

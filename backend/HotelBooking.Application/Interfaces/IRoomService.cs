using HotelBooking.Application.DTOs.Rooms;

namespace HotelBooking.Application.Interfaces;

public interface IRoomService
{
    Task<IEnumerable<RoomResponse>> GetAvailableRoomAsync(int hotelId, DateTimeOffset start, DateTimeOffset end);
    Task<RoomResponse?> GetRoomByIdAsync(int id);
}

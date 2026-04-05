using HotelBooking.Application.DTOs.Rooms;

namespace HotelBooking.Application.Interfaces;

public interface IRoomService
{
    Task<IEnumerable<RoomResponse>> GetAvailableRoomsByHotelIdAsync(int hotelId, DateTimeOffset checkIn, DateTimeOffset checkOut);
    Task<RoomResponse?> GetRoomByIdAsync(int id);
}

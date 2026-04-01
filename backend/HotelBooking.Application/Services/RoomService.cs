using HotelBooking.Application.DTOs.Rooms;
using HotelBooking.Application.Interfaces;

namespace HotelBooking.Application.Services;

public class RoomService : IRoomService
{
    private readonly IRoomRepository _roomRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RoomService(IRoomRepository roomRepository, IUnitOfWork unitOfWork)
    {
        _roomRepository = roomRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<RoomResponse>> GetAvailableRoomAsync(int hotelId, DateTimeOffset start, DateTimeOffset end)
    {
        // business validation
        if (start >= end)
        {
            throw new ArgumentException("Check-in date must be before check-out date.");
        }

        var rooms = await _roomRepository.GetAvailableRoomsAsync(hotelId, start, end);

        return rooms.Select(r => new RoomResponse(
            r.Id,
            r.HotelId,
            r.PricePerNight,
            r.Capacity,
            true
        ));
    }

    public async Task<RoomResponse?> GetRoomByIdAsync(int id)
    {
        var room = await _roomRepository.GetByIdAsync(id);

        if (room == null) return null;

        return new RoomResponse(
            room.Id,
            room.HotelId,
            room.PricePerNight,
            room.Capacity,
            true
        );
    }
}
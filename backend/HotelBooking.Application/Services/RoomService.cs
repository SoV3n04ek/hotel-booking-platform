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

    public async Task<IEnumerable<RoomResponse>> GetAvailableRoomsByHotelIdAsync(
        int hotelId,
        DateTimeOffset checkIn,
        DateTimeOffset checkOut,
        CancellationToken ct = default)
    {
        if (checkIn >= checkOut)
            throw new ArgumentException("Check-in date must be earlier than check-out date.");

        if (checkIn < DateTimeOffset.UtcNow)
        {
            throw new ArgumentException("Cannot check in for a date in the past.");
        }

        var rooms = await _roomRepository.GetAvailableRoomsAsync(hotelId, checkIn, checkOut, ct);

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
using HotelBooking.Application.DTOs.Rooms;
using HotelBooking.Application.Interfaces;
using HotelBooking.Domain.Entities;

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

    public async Task<RoomResponse?> GetRoomByIdAsync(int id, CancellationToken ct = default)
    {
        var room = await _roomRepository.GetByIdAsync(id, ct);

        if (room == null) return null;

        return new RoomResponse(
            room.Id,
            room.HotelId,
            room.PricePerNight,
            room.Capacity,
            true
        );
    }

    public async Task<int> CreateRoomAsync(CreateRoomRequest request, CancellationToken ct = default)
    {
        var room = new Room()
        {
            HotelId = request.HotelId,
            PricePerNight = request.PricePerNight,
            Capacity = request.Capacity
        };

        await _roomRepository.AddAsync(room, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return room.Id;
    }

    public async Task UpdateRoomAsync(int id, UpdateRoomRequest request, CancellationToken ct = default)
    {
        var room = await _roomRepository.GetByIdAsync(id, ct);
        if (room == null)
        {
            throw new KeyNotFoundException("Room not found");
        }

        room.PricePerNight = request.PricePerNight;
        room.Capacity = request.Capacity;

        _roomRepository.Update(room);
        await _unitOfWork.SaveChangesAsync(ct);
    }
    /*
    public async Task DeleteRoomAsync(int id, CancellationToken ct = default)
    {
        var room = _roomRepository.GetByIdAsync(id, ct);

        if (room == null)
        {
            throw new Exception("There is no room with this id for deletion");
        }

        _roomRepository.Delete(room);
    }
    */
}
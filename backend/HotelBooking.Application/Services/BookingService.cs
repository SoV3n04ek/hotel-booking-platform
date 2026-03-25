using HotelBooking.Application.Interfaces;
using HotelBooking.Domain.Entities;

namespace HotelBooking.Application.Services;

public class BookingService : IBookingService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IRoomRepository _roomRepository;
    private readonly IUnitOfWork _unitOfWork;

    public BookingService(IBookingRepository bookingRepository, IRoomRepository roomRepository, IUnitOfWork unitOfWork)
    {
        _bookingRepository = bookingRepository;
        _roomRepository = roomRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Booking> CreateBookingAsync(int userId, int roomId, DateTimeOffset checkIn, DateTimeOffset checkOut)
    {
        bool isAvailable = await _bookingRepository.IsRoomAvailableAsync(roomId, checkIn, checkOut);
        if (isAvailable is false)
        {
            throw new Exception("The room has already been booked for those dates.");
        }

        var room = await _roomRepository.GetByIdAsync(roomId);
        if (room == null)
            throw new Exception("Room hasn't founded.");

        var booking = new Booking
        {
            UserId = userId,
            RoomId = roomId,
            DateCheckIn = checkIn,
            DateCheckOut = checkOut
        };
        booking.CalculateTotalPrice(room.PricePerNight);

        await _bookingRepository.AddAsync(booking);
        await _unitOfWork.SaveChangesAsync();

        return booking;
    }

    public async Task<Booking?> GetBookingByIdAsync(int id)
    {
        return await _bookingRepository.GetByIdAsync(id);
    }
}

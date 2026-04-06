using HotelBooking.Application.DTOs.Bookings;
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

    public async Task<BookingResponse> CreateBookingAsync(CreateBookingRequest request)
    {
        if (request.CheckIn < DateTimeOffset.UtcNow)
            throw new ArgumentException("Cannot book for past dates.");

        bool isAvailable = await _roomRepository.IsRoomAvailableAsync(
            request.RoomId, request.CheckIn, request.CheckOut);

        if (!isAvailable)
            throw new InvalidOperationException("Room is already occupied for these dates.");

        var room = await _roomRepository.GetByIdAsync(request.RoomId);
        if (room == null) throw new KeyNotFoundException("Room not found.");

        var totalDays = (request.CheckOut - request.CheckIn).Days;
        if (totalDays <= 0) throw new ArgumentException("Minimum stay is 1 night.");

        var booking = new Booking
        {
            UserId = request.UserId,
            RoomId = request.RoomId,
            DateCheckIn = request.CheckIn,
            DateCheckOut = request.CheckOut,
            TotalPrice = totalDays * room.PricePerNight,
            Status = BookingStatus.Confirmed
        };

        await _bookingRepository.AddAsync(booking);
        await _unitOfWork.SaveChangesAsync();

        return new BookingResponse(
            booking.Id,
            booking.RoomId,
            booking.UserId,
            booking.DateCheckIn,
            booking.DateCheckOut,
            booking.TotalPrice,
            booking.Status.ToString()
        );
    }

    public async Task<Booking?> GetBookingByIdAsync(int id)
    {
        return await _bookingRepository.GetByIdAsync(id);
    }
}

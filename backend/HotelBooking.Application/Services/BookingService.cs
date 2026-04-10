using FluentValidation;
using HotelBooking.Application.DTOs.Bookings;
using HotelBooking.Application.Interfaces;
using HotelBooking.Application.Mappers;
using HotelBooking.Application.Validators;
using HotelBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Application.Services;

public class BookingService : IBookingService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IRoomRepository _roomRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<CreateBookingRequest> _validator;

    public BookingService(
        IBookingRepository bookingRepository, 
        IRoomRepository roomRepository, 
        IUnitOfWork unitOfWork, 
        IValidator<CreateBookingRequest> validator)
    {
        _bookingRepository = bookingRepository;
        _roomRepository = roomRepository;
        _unitOfWork = unitOfWork;
        _validator = validator;
    }

    public async Task<BookingResponse> CreateBookingAsync(CreateBookingRequest request)
    {
        await _validator.ValidateAndThrowAsync(request);

        bool isAvailable = await _roomRepository.IsRoomAvailableAsync(
            request.RoomId, request.CheckIn, request.CheckOut);

        if (!isAvailable)
            throw new InvalidOperationException("Room is already occupied for these dates.");

        var room = await _roomRepository.GetByIdAsync(request.RoomId);
        if (room == null) throw new KeyNotFoundException("Room not found.");
        _roomRepository.Update(room);

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

        try
        {
            await _unitOfWork.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new InvalidOperationException("This room was just booked by someone else. Please refresh.");
        }

        return booking.ToResponse();
    }

    public async Task<Booking?> GetBookingByIdAsync(int id)
    {
        return await _bookingRepository.GetByIdAsync(id);
    }
}

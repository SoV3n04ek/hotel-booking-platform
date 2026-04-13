using FluentValidation;
using HotelBooking.Application.DTOs.Bookings;
using HotelBooking.Application.Interfaces;
using HotelBooking.Application.Mappers;
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

    public async Task<BookingResponse> CreateBookingAsync(
        CreateBookingRequest request,
        CancellationToken ct = default)
    {
        await _validator.ValidateAndThrowAsync(request, ct);

        var room = await _roomRepository.GetRoomIfAvailableAsync(
            request.RoomId, request.CheckIn, request.CheckOut, ct);

        if (room == null)
        {
            var exists = await _roomRepository.GetAll().AnyAsync(r => r.Id == request.RoomId, ct);
            if (!exists)
                throw new KeyNotFoundException("Room not found.");

            throw new InvalidOperationException("Room is already occupied for these dates.");
        }
        _roomRepository.Update(room);

        var booking = new Booking
        {
            UserId = request.UserId,
            RoomId = request.RoomId,
            DateCheckIn = request.CheckIn,
            DateCheckOut = request.CheckOut,
            Status = BookingStatus.Confirmed
        };

        booking.CalculateTotalPrice(room.PricePerNight);
        await _bookingRepository.AddAsync(booking, ct);

        try
        {
            await _unitOfWork.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new InvalidOperationException("This room was just booked by someone else. Please refresh.");
        }

        return booking.ToResponse();
    }

    public async Task<Booking?> GetBookingByIdAsync(int id, CancellationToken ct = default)
    {
        return await _bookingRepository.GetByIdAsync(id, ct);
    }
}

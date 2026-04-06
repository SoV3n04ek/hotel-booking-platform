using HotelBooking.Application.DTOs.Bookings;
using HotelBooking.Domain.Entities;

namespace HotelBooking.Application.Interfaces;

public interface IBookingService
{
    Task<BookingResponse> CreateBookingAsync(CreateBookingRequest request);
    Task<Booking?> GetBookingByIdAsync(int id);
}

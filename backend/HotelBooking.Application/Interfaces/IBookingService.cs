using HotelBooking.Domain.Entities;

namespace HotelBooking.Application.Interfaces;

public interface IBookingService
{
    Task<Booking> CreateBookingAsync(int userId, int roomId, DateTimeOffset checkIn, DateTimeOffset checkOut);
    Task<Booking?> GetBookingByIdAsync(int id);
}

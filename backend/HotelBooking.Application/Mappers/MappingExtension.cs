using HotelBooking.Application.DTOs.Bookings;
using HotelBooking.Application.DTOs.Hotels;
using HotelBooking.Domain.Entities;

namespace HotelBooking.Application.Mappers;

public static class MappingExtension
{
    // Mapping for Hotels
    public static HotelResponse ToResponse(this Hotel hotel)
    {
        return new HotelResponse(
            hotel.Id,
            hotel.Name,
            hotel.Address,
            hotel.Description,
            hotel.Rooms?.Count ?? 0
        );
    }

    public static BookingResponse ToResponse(this Booking booking)
    {
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
}

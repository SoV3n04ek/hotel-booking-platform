using HotelBooking.Application.DTOs.Hotels;

namespace HotelBooking.Application.Interfaces;

public interface IHotelService
{
    Task<IEnumerable<HotelResponse>> SearchHotelsAsync(string? city, string? searchTerm);
    Task<HotelResponse?> GetByIdAsync(int id);
    Task<int> CreateHotelAsync(CreateHotelRequest request);
}

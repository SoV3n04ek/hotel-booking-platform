using HotelBooking.Application.DTOs;
using HotelBooking.Application.DTOs.Hotels;

namespace HotelBooking.Application.Interfaces;

public interface IHotelService
{
    Task<PagedResult<HotelResponse>> SearchHotelsAsync(string? city, int pageNumber = 1, int pageSize = 10);
    Task<HotelResponse?> GetByIdAsync(int id);
    Task<int> CreateHotelAsync(CreateHotelRequest request);
}

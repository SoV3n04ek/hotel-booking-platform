using HotelBooking.Application.DTOs;
using HotelBooking.Application.DTOs.Hotels;

namespace HotelBooking.Application.Interfaces;

public interface IHotelService
{
    Task<PagedResult<HotelResponse>> SearchHotelsAsync(HotelSearchParameters parameters);
    Task<HotelResponse?> GetByIdAsync(int id);
    Task<int> CreateHotelAsync(CreateHotelRequest request);
}
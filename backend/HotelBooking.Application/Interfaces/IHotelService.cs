using HotelBooking.Application.DTOs;
using HotelBooking.Application.DTOs.Hotels;

namespace HotelBooking.Application.Interfaces;

public interface IHotelService
{
    Task<PagedResult<HotelResponse>> SearchHotelsAsync(HotelSearchParameters parameters, CancellationToken ct = default);
    Task<HotelResponse?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<int> CreateHotelAsync(CreateHotelRequest request, CancellationToken ct = default);
}
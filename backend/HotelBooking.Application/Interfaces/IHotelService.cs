using HotelBooking.Application.DTOs;
using HotelBooking.Application.DTOs.Hotels;
using Microsoft.AspNetCore.Http;

namespace HotelBooking.Application.Interfaces;

public interface IHotelService
{
    Task<PagedResult<HotelResponse>> SearchHotelsAsync(HotelSearchParameters parameters, CancellationToken ct = default);
    Task<HotelResponse?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<int> CreateHotelAsync(CreateHotelRequest request, CancellationToken ct = default);
    Task<Guid> AddImageAsync(int hotelId, IFormFile file, string altText, bool IsPrimary, CancellationToken ct = default);
    Task<HotelImageResponse?> GetImageMetadataAsync(Guid imageId, CancellationToken ct = default);
}
using HotelBooking.Application.DTOs;
using HotelBooking.Application.DTOs.Hotels;
using HotelBooking.Application.Interfaces;
using HotelBooking.Application.Mappers;
using HotelBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Application.Services;

public class HotelService : IHotelService
{
    private readonly IHotelRepository _hotelRepository;
    private readonly IUnitOfWork _unitOfWork;
    
    public HotelService(IHotelRepository hotelRepository, IUnitOfWork unitOfWork)
    {
        _hotelRepository = hotelRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<HotelResponse>> SearchHotelsAsync(
        HotelSearchParameters parameters,
        CancellationToken ct = default)
    {
        var query = _hotelRepository.GetAll().AsNoTracking();

        if (!string.IsNullOrWhiteSpace(parameters.City))
            query = query.Where(h => h.Address.Contains(parameters.City));

        if (!string.IsNullOrEmpty(parameters.SearchTerm))
            query = query.Where(h => h.Name.Contains(parameters.SearchTerm) || h.Description.Contains(parameters.SearchTerm));

        query = parameters.SortBy?.ToLower() switch
        {
            "name" => parameters.SortOrder == "desc" ? query.OrderByDescending(h => h.Name) : query.OrderBy(h => h.Name),
            "address" => parameters.SortOrder == "desc" ? query.OrderByDescending(h => h.Address) : query.OrderBy(h => h.Address),
            _ => query.OrderBy(h => h.Id)
        };

        // Pagination
        var totalCount = await query.CountAsync(ct);

        var items = await query
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .Select(h => h.ToResponse())
            .ToListAsync();

        var totalPages = (int)Math.Ceiling(totalCount / (double)parameters.PageSize);

        return new PagedResult<HotelResponse>(items, parameters.PageNumber, parameters.PageSize, totalCount, totalPages);
    }

    public async Task<HotelResponse?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var hotel = await _hotelRepository.GetByIdAsync(id, ct);
        if (hotel == null)
            return null;

        return hotel.ToResponse();
    }

    public async Task<int> CreateHotelAsync(
        CreateHotelRequest request,
        CancellationToken ct = default)
    {
        var hotel = new Hotel
        {
            Name = request.Name,
            Address = request.Address,
            Description = request.Description
        };

        await _hotelRepository.AddAsync(hotel, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return hotel.Id;
    }
}

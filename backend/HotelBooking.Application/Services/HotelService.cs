using HotelBooking.Application.DTOs.Hotels;
using HotelBooking.Application.Interfaces;
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

    public async Task<IEnumerable<HotelResponse>> SearchHotelsAsync(string? city, string? searchTerm)
    {
        IQueryable<Hotel> query = _hotelRepository.GetAll().AsNoTracking();

        if (!string.IsNullOrWhiteSpace(city))
        {
            query = query.Where(h => h.Address.Contains(city));
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(h => h.Name.Contains(searchTerm) || h.Description.Contains(searchTerm));
        }

        return await query
            .Select(h => new HotelResponse(
                h.Id,
                h.Name,
                h.Address,
                h.Description,
                h.Rooms.Count))
            .ToListAsync();
    }

    public async Task<HotelResponse?> GetByIdAsync(int id)
    {
        var hotel = await _hotelRepository.GetByIdAsync(id);
        if (hotel == null)
            return null;

        return new HotelResponse(
            hotel.Id,
            hotel.Name,
            hotel.Address,
            hotel.Description,
            0);
    }

    public async Task<int> CreateHotelAsync(CreateHotelRequest request)
    {
        var hotel = new Hotel
        {
            Name = request.Name,
            Address = request.Address,
            Description = request.Description
        };

        await _hotelRepository.AddAsync(hotel);
        await _unitOfWork.SaveChangesAsync();

        return hotel.Id;
    }
}

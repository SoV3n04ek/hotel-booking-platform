using HotelBooking.Application.DTOs;
using HotelBooking.Application.DTOs.Hotels;
using HotelBooking.Application.Interfaces;
using HotelBooking.Application.Mappers;
using HotelBooking.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Application.Services;

public class HotelService : IHotelService
{
    private readonly IHotelRepository _hotelRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileService _fileService;

    public HotelService(IHotelRepository hotelRepository, IUnitOfWork unitOfWork, IFileService fileService)
    {
        _hotelRepository = hotelRepository;
        _unitOfWork = unitOfWork;
        _fileService = fileService;
    }

    public async Task<PagedResult<HotelResponse>> SearchHotelsAsync(
        HotelSearchParameters parameters,
        CancellationToken ct = default)
    {
        var query = _hotelRepository.GetAll().AsNoTracking();

        // City filter
        if (!string.IsNullOrWhiteSpace(parameters.City))
        {
            var cityTerm = parameters.City.ToLower();
            query = query.Where(h => h.Address.ToLower().Contains(cityTerm));
        }

        // Search term filter
        if (!string.IsNullOrEmpty(parameters.SearchTerm))
        {
            var term = parameters.SearchTerm.ToLower();
            query = query.Where(h =>
                h.Name.ToLower().Contains(term) ||
                h.Description.ToLower().Contains(term));
        }

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

    public async Task AddImageAsync(
        int hotelId, 
        IFormFile file, 
        string altText, 
        bool isPrimary, 
        CancellationToken ct = default)
    {
        if (file == null || file.Length == 0)
        {
            throw new ArgumentException("File is empty or null");
        }

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension))
        {
            throw new ArgumentException("Invalid file format. Only JPG, PNG, and WEBP are allowed.");
        }

        var hotel = await _hotelRepository.GetByIdAsync(hotelId, ct);
        if (hotel == null)
        {
            throw new KeyNotFoundException($"Hotel with ID{hotelId} not found.");
        }

        string? savedFileUrl = null;
        try
        {
            savedFileUrl = await _fileService.SaveFileAsync(file, "hotels", ct);

            if (isPrimary)
            {
                // Unset other primary images for this hotel
                foreach (var img in hotel.Images.Where(i => i.IsPrimary))
                {
                    img.IsPrimary = false;
                }
            }

            var hotelImage = new HotelImage
            {
                HotelId = hotelId,
                Url = savedFileUrl,
                AltText = altText,
                IsPrimary = isPrimary,
                DisplayOrder = hotel.Images.Count + 1
            };

            hotel.Images.Add(hotelImage);

            await _unitOfWork.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            if (savedFileUrl != null)
            {
                await _fileService.DeleteFileAsync(savedFileUrl, ct);
            }

            // Re-throw to be caught by global exception handler
            throw; 
        }

    }
}

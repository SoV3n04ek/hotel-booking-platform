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
    private readonly IHotelImageRepository _hotelImageRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileService _fileService;

    public HotelService(
        IHotelRepository hotelRepository, 
        IHotelImageRepository hotelImageRepository, 
        IUnitOfWork unitOfWork, 
        IFileService fileService)
    {
        _hotelRepository = hotelRepository;
        _unitOfWork = unitOfWork;
        _fileService = fileService;
        _hotelImageRepository = hotelImageRepository;
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

    public async Task<Guid> AddImageAsync(
        int hotelId, 
        IFormFile file, 
        string altText, 
        bool isPrimary, 
        CancellationToken ct = default)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("File is empty");

        var hotel = await _hotelRepository.GetByIdAsync(hotelId, ct);
        if (hotel == null)
            throw new KeyNotFoundException($"Hotel {hotelId} not found");

        string? savedFileUrl = null;
        try
        {
            savedFileUrl = await _fileService.SaveFileAsync(file, "hotels", ct);

            if (isPrimary)
            {
                await _hotelImageRepository.UnsetPrimaryImagesAsync(hotelId, ct);
            }

            var hotelImage = new HotelImage
            {
                Id = Guid.NewGuid(),
                HotelId = hotelId,
                Url = savedFileUrl,
                AltText = altText,
                IsPrimary = isPrimary,
                DisplayOrder = 0
            };

            await _hotelImageRepository.AddAsync(hotelImage, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            return hotelImage.Id;
        }
        catch (Exception)
        {
            if (savedFileUrl != null)
                await _fileService.DeleteFileAsync(savedFileUrl, ct);
            
            throw;
        }
    }

    public async Task<HotelImageResponse?> GetImageMetadataAsync(Guid imageId, CancellationToken ct = default)
    {
        // TODO: Automatically transform the relative path into a full, clickable URL before it leaves the API.
        var image = await _hotelImageRepository.GetByIdAsync(imageId, ct);

        if (image == null)
            return null;

        return new HotelImageResponse(
            image.Id,
            image.HotelId,
            image.Url,
            image.AltText,
            image.IsPrimary,
            image.DisplayOrder);
    }
}

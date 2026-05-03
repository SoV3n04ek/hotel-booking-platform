using HotelBooking.Domain.Entities;

namespace HotelBooking.Application.Interfaces;

public interface IHotelImageRepository
{
    Task AddAsync(HotelImage image, CancellationToken ct);
    Task<HotelImage?> GetByIdAsync(Guid id, CancellationToken ct);
    Task UnsetPrimaryImagesAsync(int hotelId, CancellationToken ct);
    void Delete(HotelImage image);
}

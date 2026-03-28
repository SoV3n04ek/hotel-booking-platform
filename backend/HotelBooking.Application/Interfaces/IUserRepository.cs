using HotelBooking.Domain.Entities;

namespace HotelBooking.Application.Interfaces;

public interface IUserRepository : IGenericRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
}

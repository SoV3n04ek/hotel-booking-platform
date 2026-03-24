using HotelBooking.Domain.Entities;

namespace HotelBooking.Application.Interfaces;

internal interface IUserRepository : IGenericRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
}

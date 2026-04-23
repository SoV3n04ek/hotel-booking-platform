using HotelBooking.Domain.Entities;
using HotelBooking.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HotelBooking.Infrastructure.Persistence;

public class DbInitializer
{
    private readonly HotelsDbContext _context;
    private readonly ILogger<DbInitializer> _logger;

    public DbInitializer(HotelsDbContext context, ILogger<DbInitializer> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        try
        {
            if (_context.Database.IsRelational())
            {
                await _context.Database.MigrateAsync();
            }

            if (await _context.Hotels.AnyAsync())
            {
                _logger.LogInformation("Database already seeded.");
                return;
            }

            var adminUser = new User
            {
                Name = "System Admin",
                Email = "admin@hotelbooking.com",
                PasswordHash = "hashed_password_placeholder", 
                Role = UserRole.Admin
            };

            var standardUser = new User
            {
                Name = "John Doe",
                Email = "john.doe@example.com",
                PasswordHash = "hashed_password_placeholder",
                Role = UserRole.User
            };

            await _context.Users.AddRangeAsync(adminUser, standardUser);

            var hotel1 = new Hotel
            {
                Name = "Grand Plaza Hotel",
                Address = "123 Main St, City Center",
                Description = "Luxury hotel in the heart of the city.",
                Rooms = new List<Room>
                {
                    new Room { PricePerNight = 150.00m, Capacity = 2 },
                    new Room { PricePerNight = 250.00m, Capacity = 4 }
                }
            };

            var hotel2 = new Hotel
            {
                Name = "Seaside Resort",
                Address = "45 Ocean Drive, Beachfront",
                Description = "Relaxing resort with ocean views.",
                Rooms = new List<Room>
                {
                    new Room { PricePerNight = 120.00m, Capacity = 2 },
                    new Room { PricePerNight = 180.00m, Capacity = 3 }
                }
            };

            await _context.Hotels.AddRangeAsync(hotel1, hotel2);

            await _context.SaveChangesAsync();
            _logger.LogInformation("Database seeding completed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }
}
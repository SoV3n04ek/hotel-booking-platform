using HotelBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Infrastructure;

public class HotelsDbContext : DbContext
{
    public HotelsDbContext(DbContextOptions<HotelsDbContext> options) : base(options) { }

    public DbSet<Hotel> Hotels => Set<Hotel>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Booking> Bookings => Set<Booking>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Hotel>(entity =>
        {
            entity.HasKey(h => h.Id);
            entity.Property(h => h.Name).IsRequired().HasMaxLength(64);
            entity.Property(h => h.Address).IsRequired().HasMaxLength(256);
            entity.Property(h => h.Description).IsRequired().HasMaxLength(512);
        });

        // TODO: Rules for Room, User, Booking

        modelBuilder.Entity<User>()
            .Property(u => u.Role)
            .HasConversion<string>();

        base.OnModelCreating(modelBuilder);
    }
}
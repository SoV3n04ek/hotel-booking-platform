using HotelBooking.Application.Interfaces;
using HotelBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Infrastructure;

public class HotelsDbContext : DbContext, IUnitOfWork
{
    public HotelsDbContext(DbContextOptions<HotelsDbContext> options) : base(options) { }

    public DbSet<Hotel> Hotels => Set<Hotel>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Booking> Bookings => Set<Booking>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Hotel
        modelBuilder.Entity<Hotel>(entity =>
        {
            entity.HasKey(h => h.Id);

            entity.Property(h => h.Name)
                .IsRequired()
                .HasMaxLength(64);
            
            entity.Property(h => h.Address)
                .IsRequired()
                .HasMaxLength(256);
            
            entity.Property(h => h.Description)
                .IsRequired()
                .HasMaxLength(1024);

            // 1:N (1 hotel -> many rooms)
            entity.HasMany(h => h.Rooms)
                .WithOne(r => r.Hotel)
                .HasForeignKey(r => r.HotelId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Room 
        modelBuilder.Entity<Room>(entity =>
        {
            entity.HasKey(r => r.Id);
            
            entity.Property(r => r.PricePerNight)
                .HasPrecision(18,2)
                .IsRequired();
            
            entity.Property(r => r.Capacity).IsRequired();

            // 1:N (1 Room -> many Bookings)
            entity.HasMany(r => r.Bookings)
                .WithOne(b => b.Room)
                .HasForeignKey(b => b.RoomId);
        });

        // TODO: Rules for Room, User, Booking

        // User 
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            
            entity.Property(u => u.Name)
                .IsRequired()
                .HasMaxLength(50);
            
            entity.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(100);
            
            entity.HasIndex(u => u.Email)
                .IsUnique();

            // Enum role to string
            entity.Property(u => u.Role)
                .HasConversion<string>()
                .HasMaxLength(20);

            // 1:N (1 User -> many bookings)
            entity.HasMany(u => u.Bookings)
                .WithOne(b => b.User)
                .HasForeignKey(b => b.UserId);
        });

        // Booking
        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(b => b.Id);
           
            entity.Property(b => b.TotalPrice)
                .HasPrecision(18, 2)
                .IsRequired();
            
            entity.Property(b => b.DateCheckIn).IsRequired();

            entity.Property(b => b.DateCheckOut).IsRequired();
        });        
    }
}
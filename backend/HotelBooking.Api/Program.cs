
using HotelBooking.Application.Interfaces;
using HotelBooking.Application.Services;
using HotelBooking.Infrastructure;
using HotelBooking.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

namespace HotelBooking.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

        builder.Services.AddDbContext<HotelsDbContext>(options =>
            options.UseNpgsql(connectionString)
                   .UseSnakeCaseNamingConvention()
        );

        // Infrastracture
        builder.Services.AddScoped<IBookingRepository, BookingRepository>();
        builder.Services.AddScoped<IRoomRepository, RoomRepository>();
        builder.Services.AddScoped<IHotelRepository, HotelRepository>();
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        
        // Application
        builder.Services.AddScoped<IBookingService, BookingService>();
        builder.Services.AddScoped<IHotelService, HotelService>();

        // Database and Unit of Work
        builder.Services.AddScoped<IUnitOfWork, HotelsDbContext>();

        builder.Services.AddControllers();
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        var app = builder.Build();

        app.UseExceptionHandler();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();


        app.MapControllers();

        app.Run();
    }
}

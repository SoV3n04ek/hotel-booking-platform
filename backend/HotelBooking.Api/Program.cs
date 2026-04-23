
using System.Threading.Tasks;
using FluentValidation;
using HotelBooking.Api.Middleware;
using HotelBooking.Application.Interfaces;
using HotelBooking.Application.Services;
using HotelBooking.Application.Validators;
using HotelBooking.Infrastructure;
using HotelBooking.Infrastructure.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Scalar.AspNetCore;

namespace HotelBooking.Api;

public class Program
{
    public static async Task Main(string[] args)
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

        builder.Services.AddScoped<IRoomService, RoomService>();
        // Validation
        builder.Services.AddValidatorsFromAssemblyContaining<CreateBookingRequestValidator>();
        builder.Services.AddValidatorsFromAssemblyContaining<BookingValidator>();

        // Database and Unit of Work
        builder.Services.AddScoped<IUnitOfWork, HotelsDbContext>();

        builder.Services.AddControllers();

        builder.Services.AddProblemDetails();
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();
        
        builder.Services.AddScoped<HotelBooking.Infrastructure.Persistence.DbInitializer>();

        var app = builder.Build();

        app.UseExceptionHandler();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();

            using var scope = app.Services.CreateScope();
            var initializer = scope.ServiceProvider.GetRequiredService<HotelBooking.Infrastructure.Persistence.DbInitializer>();
            await initializer.SeedAsync();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();


        app.MapControllers();

        app.Run();
    }
}

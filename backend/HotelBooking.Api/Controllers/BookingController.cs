using System.Threading;
using System.Threading.Tasks;
using HotelBooking.Application.DTOs.Bookings;
using HotelBooking.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HotelBooking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookingController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBookingRequest request, CancellationToken ct = default)
    {
        var booking = await _bookingService.CreateBookingAsync(request, ct);

        return CreatedAtAction(nameof(GetById), new { id = booking.Id }, booking); 
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct = default)
    {
        var booking = await _bookingService.GetBookingByIdAsync(id, ct);
        
        if (booking == null) 
            return NotFound(new { message = $"Booking with ID {id} not found." });

        return Ok(booking);
    }
}
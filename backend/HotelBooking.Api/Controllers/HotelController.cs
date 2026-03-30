using HotelBooking.Application.DTOs.Hotels;
using HotelBooking.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HotelBooking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HotelController : ControllerBase
{
    private readonly IHotelService _hotelService;

    public HotelController(IHotelService hotelService)
    {
        _hotelService = hotelService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateHotelRequest request)
    {
        if (request == null) return BadRequest();

        var hotelId = await _hotelService.CreateHotelAsync(request);

        return CreatedAtAction(nameof(GetById), new { id = hotelId }, request);
    }

    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<HotelResponse>>> Search(
        [FromQuery] string? city,
        [FromQuery] string? searchTerm)
    {
        var results = await _hotelService.SearchHotelsAsync(city, searchTerm);
        return Ok(results);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<HotelResponse>> GetById(int id)
    {
        var hotel = await _hotelService.GetByIdAsync(id);

        if (hotel == null) return NotFound(new { message = "Hotel not found" });

        return Ok(hotel);
    }
}

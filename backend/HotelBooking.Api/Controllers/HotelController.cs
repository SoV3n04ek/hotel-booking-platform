using HotelBooking.Application.DTOs;
using HotelBooking.Application.DTOs.Hotels;
using HotelBooking.Application.DTOs.Rooms;
using HotelBooking.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HotelBooking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HotelController : ControllerBase
{
    private readonly IHotelService _hotelService;
    private readonly IRoomService _roomsService;
   
    public HotelController(IHotelService hotelService, IRoomService roomService)
    {
        _hotelService = hotelService;
        _roomsService = roomService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateHotelRequest request)
    {
        if (request == null) return BadRequest();

        var hotelId = await _hotelService.CreateHotelAsync(request);

        return CreatedAtAction(nameof(GetById), new { id = hotelId }, request);
    }

    [HttpGet("search")]
    public async Task<ActionResult<PagedResult<HotelResponse>>> Search(
        [FromQuery] HotelSearchParameters parameters)
    {
        var validatedParams = parameters with
        {
            PageNumber = parameters.PageNumber < 1 ? 1 : parameters.PageNumber,
            PageSize = (parameters.PageSize < 1 || parameters.PageSize > 50) ? 10 : parameters.PageSize
        };

        var results = await _hotelService.SearchHotelsAsync(validatedParams);
        return Ok(results);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<HotelResponse>> GetById(int id)
    {
        var hotel = await _hotelService.GetByIdAsync(id);

        if (hotel == null) return NotFound(new { message = "Hotel not found" });

        return Ok(hotel);
    }

    [HttpGet("{id}/rooms")]
    public async Task<ActionResult<IEnumerable<RoomResponse>>> GetAvailableRooms(
        int id,
        [FromQuery] DateTimeOffset checkIn,
        [FromQuery] DateTimeOffset checkOut)
    {
        var hotel = await _hotelService.GetByIdAsync(id);
        if (hotel == null)
        {
            return NotFound(new { message = $"Hotel with Id {id} not found." });
        }

        var rooms = await _roomsService.GetAvailableRoomsByHotelIdAsync(id, checkIn, checkOut);
        return Ok(rooms);
    }
}

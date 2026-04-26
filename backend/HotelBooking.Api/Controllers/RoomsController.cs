using System.Threading.Tasks;
using HotelBooking.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using HotelBooking.Application.DTOs.Rooms;

namespace HotelBooking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RoomsController : ControllerBase
{
    private readonly IRoomService _roomService;

    public RoomsController(IRoomService roomService)
    {
        _roomService = roomService;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<RoomResponse>> GetById(int id)
    {
        var room = await _roomService.GetRoomByIdAsync(id);
        
        if (room == null)
            return NotFound();
        
        return Ok(room);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRoomRequest request)
    {
        var roomId = await _roomService.CreateRoomAsync(request);
        var response = await _roomService.GetRoomByIdAsync(roomId);
        return CreatedAtAction(nameof(GetById), new { id = roomId }, response);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateRoomRequest request)
    {
        await _roomService.UpdateRoomAsync(id, request);
        return NoContent();
    }

    [HttpDelete]
    public async Task<IActionResult> Delete(int id)
    {
        await _roomService.DeleteRoomAsync(id);
        return NoContent();
    }
}
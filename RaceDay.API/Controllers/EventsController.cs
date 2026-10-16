using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;
using System.Security.Claims;

namespace RaceDay.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EventsController(RaceDayDbContext context)
        {
            _context = context;
        }

        // GET: api/events (Both roles can view)
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Event>>> GetEvents()
        {
            return await _context.Events.Include(e => e.Organiser).ToListAsync();
        }

        // GET: api/events/5
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<Event>> GetEvent(int id)
        {
            var @event = await _context.Events.Include(e => e.Categories).FirstOrDefaultAsync(e => e.EventId == id);
            if (@event == null) return NotFound("Event not found.");
            return @event;
        }

        // POST: api/events (Organiser only)
        [HttpPost]
        [Authorize(Roles = "Organiser")]
        public async Task<ActionResult<Event>> CreateEvent(CreateEventDto request)
        {
            var organiserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

            var newEvent = new Event
            {
                OrganiserId = organiserId,
                EventName = request.EventName,
                EventDate = request.EventDate,
                Location = request.Location,
                MaxParticipants = request.MaxParticipants,
                Status = "Upcoming"
            };

            _context.Events.Add(newEvent);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEvent), new { id = newEvent.EventId }, newEvent);
        }

        // PUT: api/events/5 (Organiser only, must own event)
        [HttpPut("{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> UpdateEvent(int id, CreateEventDto request)
        {
            var organiserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var existingEvent = await _context.Events.FindAsync(id);

            if (existingEvent == null) return NotFound("Event not found.");
            if (existingEvent.OrganiserId != organiserId) return Forbid("You can only edit your own events.");

            existingEvent.EventName = request.EventName;
            existingEvent.EventDate = request.EventDate;
            existingEvent.Location = request.Location;
            existingEvent.MaxParticipants = request.MaxParticipants;

            await _context.SaveChangesAsync();
            return Ok(new { Message = "Event updated successfully." });
        }

        // DELETE: api/events/5 (Organiser only, must own event)
        [HttpDelete("{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var organiserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var existingEvent = await _context.Events.FindAsync(id);

            if (existingEvent == null) return NotFound("Event not found.");
            if (existingEvent.OrganiserId != organiserId) return Forbid("You can only delete your own events.");

            _context.Events.Remove(existingEvent);
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Event deleted successfully." });
        }
    }
}
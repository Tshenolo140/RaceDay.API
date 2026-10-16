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
    public class EnrolmentsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EnrolmentsController(RaceDayDbContext context) => _context = context;

        [HttpPost]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> CreateEnrolment(CreateEnrolmentDto request)
        {
            var participantId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

            var eventItem = await _context.Events.FindAsync(request.EventId);
            if (eventItem == null) return NotFound("Event not found.");

            var currentEnrolments = await _context.Enrolments.CountAsync(e => e.EventId == request.EventId);
            if (currentEnrolments >= eventItem.MaxParticipants) return BadRequest("Event is full.");

            //Check if category exists and belongs to this event
            var category = await _context.Categories.FindAsync(request.CategoryId);
            if (category == null || category.EventId != request.EventId)
                return BadRequest("Invalid Category for this Event.");

            //Check for duplicates
            var alreadyEnrolled = await _context.Enrolments.AnyAsync(e =>
                e.EventId == request.EventId && e.ParticipantId == participantId);
            if (alreadyEnrolled) return BadRequest("You are already enrolled in this event.");

            //Create Enrolment
            var enrolment = new Enrolment
            {
                EventId = request.EventId,
                ParticipantId = participantId,
                CategoryId = request.CategoryId,
                PaymentStatus = "Pending",
                AmountPaid = 0.00m,
                BibNumber = new Random().Next(1000, 9999) 
            };

            _context.Enrolments.Add(enrolment);
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Enrolled successfully!", BibNumber = enrolment.BibNumber });
        }

        // GET: api/enrolments/my-enrolments (Participant only)
        [HttpGet("my-enrolments")]
        [Authorize(Roles = "Participant")]
        public async Task<ActionResult<IEnumerable<Enrolment>>> GetMyEnrolments()
        {
            var participantId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            return await _context.Enrolments
                .Include(e => e.Event)
                .Include(e => e.Category)
                .Where(e => e.ParticipantId == participantId)
                .ToListAsync();
        }

        // GET: api/enrolments/event/5 (Organiser only)
        [HttpGet("event/{eventId}")]
        [Authorize(Roles = "Organiser")]
        public async Task<ActionResult<IEnumerable<Enrolment>>> GetEnrolmentsForEvent(int eventId)
        {
            var organiserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

            var eventItem = await _context.Events.FindAsync(eventId);
            if (eventItem == null || eventItem.OrganiserId != organiserId) return Forbid();

            return await _context.Enrolments
                .Include(e => e.Participant)
                .Include(e => e.Category)
                .Where(e => e.EventId == eventId)
                .ToListAsync();
        }
    }
}
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
    public class ResultsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public ResultsController(RaceDayDbContext context) => _context = context;

        // POST: api/results (Organiser only)
        [HttpPost]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> CaptureResult(CreateResultDto request)
        {
            var organiserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

            // Verify the enrolment exists and belongs to an event owned by this organiser
            var enrolment = await _context.Enrolments
                .Include(e => e.Event)
                .FirstOrDefaultAsync(e => e.EnrolmentId == request.EnrolmentId);

            if (enrolment == null) return NotFound("Enrolment not found.");
            if (enrolment.Event.OrganiserId != organiserId) return Forbid("You do not own the event for this enrolment.");

            var result = new Result
            {
                EnrolmentId = request.EnrolmentId,
                FinishTime = request.FinishTime,
                OverallRank = request.OverallRank,
                CategoryRank = request.CategoryRank,
                Status = request.Status
            };

            _context.Results.Add(result);
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Result captured successfully." });
        }

        // GET: api/results/my-results (Participant only)
        [HttpGet("my-results")]
        [Authorize(Roles = "Participant")]
        public async Task<ActionResult<IEnumerable<Result>>> GetMyResults()
        {
            var participantId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

            return await _context.Results
                .Include(r => r.Enrolment)
                .ThenInclude(e => e.Event)
                .Where(r => r.Enrolment.ParticipantId == participantId)
                .ToListAsync();
        }
    }
}
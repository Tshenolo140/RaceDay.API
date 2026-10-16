using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using System.Security.Claims;

namespace RaceDay.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Both Organisers and Participants can access this
    public class ProfileController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public ProfileController(RaceDayDbContext context) => _context = context;

        // GET: api/profile
        [HttpGet]
        public async Task<IActionResult> GetProfile()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (role == "Organiser")
            {
                var organiser = await _context.Organisers.FindAsync(userId);
                if (organiser == null) return NotFound();
                return Ok(new { organiser.FirstName, organiser.LastName, organiser.Email, organiser.Phone, organiser.Organisation });
            }
            else if (role == "Participant")
            {
                var participant = await _context.Participants.FindAsync(userId);
                if (participant == null) return NotFound();
                return Ok(new { participant.FirstName, participant.LastName, participant.Email, participant.Phone, participant.DateOfBirth, participant.Gender, participant.EmergencyContact, participant.EmergencyPhone });
            }

            return BadRequest("Invalid user role.");
        }

        // PUT: api/profile
        [HttpPut]
        public async Task<IActionResult> UpdateProfile(UpdateProfileDto request)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (role == "Organiser")
            {
                var organiser = await _context.Organisers.FindAsync(userId);
                if (organiser == null) return NotFound();

                if (!string.IsNullOrEmpty(request.FirstName)) organiser.FirstName = request.FirstName;
                if (!string.IsNullOrEmpty(request.LastName)) organiser.LastName = request.LastName;
                if (!string.IsNullOrEmpty(request.Phone)) organiser.Phone = request.Phone;
                if (!string.IsNullOrEmpty(request.Organisation)) organiser.Organisation = request.Organisation;

                await _context.SaveChangesAsync();
                return Ok(new { Message = "Organiser profile updated." });
            }
            else if (role == "Participant")
            {
                var participant = await _context.Participants.FindAsync(userId);
                if (participant == null) return NotFound();

                if (!string.IsNullOrEmpty(request.FirstName)) participant.FirstName = request.FirstName;
                if (!string.IsNullOrEmpty(request.LastName)) participant.LastName = request.LastName;
                if (!string.IsNullOrEmpty(request.Phone)) participant.Phone = request.Phone;
                if (!string.IsNullOrEmpty(request.EmergencyContact)) participant.EmergencyContact = request.EmergencyContact;
                if (!string.IsNullOrEmpty(request.EmergencyPhone)) participant.EmergencyPhone = request.EmergencyPhone;

                await _context.SaveChangesAsync();
                return Ok(new { Message = "Participant profile updated." });
            }

            return BadRequest("Invalid user role.");
        }
    }
}
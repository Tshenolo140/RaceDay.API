using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace RaceDay.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly RaceDayDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(RaceDayDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto request)
        {
            if (await _context.Organisers.AnyAsync(o => o.Email == request.Email) ||
                await _context.Participants.AnyAsync(p => p.Email == request.Email))
            {
                return BadRequest("User with this email already exists.");
            }

            //Hash the password using BCrypt
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            //Determine role and save to correct table
            if (request.Role.Equals("Organiser", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrEmpty(request.Organisation)) return BadRequest("Organisation name is required for Organisers.");

                var organiser = new Organiser
                {
                    Email = request.Email,
                    PasswordHash = passwordHash,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Phone = request.Phone,
                    Organisation = request.Organisation
                };
                _context.Organisers.Add(organiser);
            }
            else if (request.Role.Equals("Participant", StringComparison.OrdinalIgnoreCase))
            {
                if (request.DateOfBirth == null || string.IsNullOrEmpty(request.Gender))
                    return BadRequest("Date of Birth and Gender are required for Participants.");

                var participant = new Participant
                {
                    Email = request.Email,
                    PasswordHash = passwordHash,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Phone = request.Phone,
                    DateOfBirth = request.DateOfBirth.Value,
                    Gender = request.Gender,
                    EmergencyContact = request.EmergencyContact ?? "N/A",
                    EmergencyPhone = request.EmergencyPhone ?? "N/A"
                };
                _context.Participants.Add(participant);
            }
            else
            {
                return BadRequest("Invalid role. Must be 'Organiser' or 'Participant'.");
            }

            await _context.SaveChangesAsync();
            return Ok(new { Message = "User registered successfully." });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto request)
        {
            //Find user in Organiser table
            var organiser = await _context.Organisers.FirstOrDefaultAsync(o => o.Email == request.Email);
            if (organiser != null)
            {
                if (!BCrypt.Net.BCrypt.Verify(request.Password, organiser.PasswordHash))
                    return BadRequest("Invalid password.");

                var token = CreateToken(organiser.OrganiserId, organiser.Email, "Organiser");
                return Ok(new AuthResponseDto { Token = token, Role = "Organiser", UserId = organiser.OrganiserId });
            }

            //Find user in Participant table
            var participant = await _context.Participants.FirstOrDefaultAsync(p => p.Email == request.Email);
            if (participant != null)
            {
                if (!BCrypt.Net.BCrypt.Verify(request.Password, participant.PasswordHash))
                    return BadRequest("Invalid password.");

                var token = CreateToken(participant.ParticipantId, participant.Email, "Participant");
                return Ok(new AuthResponseDto { Token = token, Role = "Participant", UserId = participant.ParticipantId });
            }

            //User not found in either table
            return BadRequest("User not found.");
        }

        //Helper method to generate the JWT Token
        private string CreateToken(int userId, string email, string role)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Role, role) // This is crucial for Role-Based Access
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(1), 
                SigningCredentials = creds,
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"]
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}
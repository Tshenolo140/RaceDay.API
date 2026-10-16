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
    public class CategoriesController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public CategoriesController(RaceDayDbContext context) => _context = context;

        [HttpGet("event/{eventId}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Category>>> GetCategoriesForEvent(int eventId)
        {
            return await _context.Categories.Where(c => c.EventId == eventId).ToListAsync();
        }

        [HttpPost]
        [Authorize(Roles = "Organiser")]
        public async Task<ActionResult<Category>> CreateCategory(CreateCategoryDto request)
        {
            var organiserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

            var eventItem = await _context.Events.FindAsync(request.EventId);
            if (eventItem == null || eventItem.OrganiserId != organiserId)
                return BadRequest("Invalid Event or you do not own this event.");

            var category = new Category
            {
                EventId = request.EventId,
                CategoryName = request.CategoryName,
                Description = request.Description,
                AgeGroup = request.AgeGroup,
                GenderSpecific = request.GenderSpecific
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return Ok(category);
        }
    }
}
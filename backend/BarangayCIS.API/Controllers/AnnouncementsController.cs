using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BarangayCIS.API.Data;
using BarangayCIS.API.Models;

namespace BarangayCIS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AnnouncementsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AnnouncementsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? type, [FromQuery] string? status)
        {
            var query = _context.Announcements.AsQueryable();
            if (!string.IsNullOrEmpty(type))
            {
                query = query.Where(a => a.Type == type);
            }
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(a => a.Status == status);
            }
            var announcements = await query.OrderByDescending(a => a.CreatedAt).ToListAsync();
            return Ok(announcements);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var announcement = await _context.Announcements.FindAsync(id);
            if (announcement == null) return NotFound();
            return Ok(announcement);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateAnnouncementDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var announcement = new Announcement
                {
                    Title = dto.Title,
                    Content = dto.Content,
                    Type = dto.Type,
                    Priority = dto.Priority,
                    EventDate = dto.EventDate,
                    EventEndDate = dto.EventEndDate,
                    Location = dto.Location,
                    Status = dto.Status,
                    IsPublished = dto.IsPublished,
                    PublishedAt = dto.IsPublished ? DateTime.UtcNow : null,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Announcements.Add(announcement);
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(GetById), new { id = announcement.Id }, announcement);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the announcement", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateAnnouncementDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var announcement = await _context.Announcements.FindAsync(id);
                if (announcement == null) return NotFound();

                if (dto.Title != null) announcement.Title = dto.Title;
                if (dto.Content != null) announcement.Content = dto.Content;
                if (dto.Type != null) announcement.Type = dto.Type;
                if (dto.Priority != null) announcement.Priority = dto.Priority;
                if (dto.EventDate.HasValue) announcement.EventDate = dto.EventDate;
                if (dto.EventEndDate.HasValue) announcement.EventEndDate = dto.EventEndDate;
                if (dto.Location != null) announcement.Location = dto.Location;
                if (dto.Status != null) announcement.Status = dto.Status;
                if (dto.IsPublished.HasValue)
                {
                    announcement.IsPublished = dto.IsPublished.Value;
                    if (dto.IsPublished.Value && announcement.PublishedAt == null)
                    {
                        announcement.PublishedAt = DateTime.UtcNow;
                    }
                }
                announcement.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return Ok(announcement);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the announcement", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var announcement = await _context.Announcements.FindAsync(id);
            if (announcement == null) return NotFound();

            _context.Announcements.Remove(announcement);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}




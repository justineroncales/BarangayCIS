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
    public class ProjectsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProjectsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? status)
        {
            var query = _context.Projects.AsQueryable();
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(p => p.Status == status);
            }
            var projects = await query.OrderByDescending(p => p.CreatedAt).ToListAsync();
            return Ok(projects);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var project = await _context.Projects
                .Include(p => p.Updates)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (project == null) return NotFound();
            return Ok(project);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateProjectDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Generate project number
                var year = DateTime.Now.Year;
                var count = await _context.Projects.CountAsync(p => p.ProjectNumber.StartsWith($"PRJ-{year}"));
                var projectNumber = $"PRJ-{year}-{(count + 1):D4}";

                var project = new Project
                {
                    ProjectName = dto.ProjectName,
                    ProjectNumber = projectNumber,
                    Description = dto.Description,
                    Contractor = dto.Contractor,
                    Budget = dto.Budget,
                    StartDate = dto.StartDate,
                    TargetCompletionDate = dto.TargetCompletionDate,
                    Status = dto.Status,
                    Progress = dto.Progress,
                    Notes = dto.Notes,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Projects.Add(project);
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(GetById), new { id = project.Id }, project);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the project", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateProjectDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var project = await _context.Projects.FindAsync(id);
                if (project == null) return NotFound();

                if (dto.ProjectName != null) project.ProjectName = dto.ProjectName;
                if (dto.Description != null) project.Description = dto.Description;
                if (dto.Contractor != null) project.Contractor = dto.Contractor;
                if (dto.Budget.HasValue) project.Budget = dto.Budget.Value;
                if (dto.StartDate.HasValue) project.StartDate = dto.StartDate.Value;
                if (dto.TargetCompletionDate.HasValue) project.TargetCompletionDate = dto.TargetCompletionDate.Value;
                if (dto.ActualCompletionDate.HasValue) project.ActualCompletionDate = dto.ActualCompletionDate;
                if (dto.Status != null) project.Status = dto.Status;
                if (dto.Progress != null) project.Progress = dto.Progress;
                if (dto.Notes != null) project.Notes = dto.Notes;
                project.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return Ok(project);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the project", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null) return NotFound();

            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}




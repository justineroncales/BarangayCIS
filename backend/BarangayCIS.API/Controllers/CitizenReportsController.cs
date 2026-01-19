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
    public class CitizenReportsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CitizenReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? type, [FromQuery] string? status)
        {
            var query = _context.CitizenReports.AsQueryable();
            if (!string.IsNullOrEmpty(type))
            {
                query = query.Where(r => r.ReportType == type);
            }
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(r => r.Status == status);
            }
            var reports = await query.OrderByDescending(r => r.CreatedAt).ToListAsync();
            return Ok(reports);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var report = await _context.CitizenReports.FindAsync(id);
            if (report == null) return NotFound();
            return Ok(report);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCitizenReportDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var report = new CitizenReport
                {
                    ReportType = dto.ReportType,
                    Title = dto.Title,
                    Description = dto.Description,
                    Location = dto.Location,
                    ReporterName = dto.ReporterName,
                    ReporterContact = dto.ReporterContact,
                    Status = dto.Status,
                    AssignedTo = dto.AssignedTo,
                    CreatedAt = DateTime.UtcNow
                };

                _context.CitizenReports.Add(report);
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(GetById), new { id = report.Id }, report);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the report", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCitizenReportDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var report = await _context.CitizenReports.FindAsync(id);
                if (report == null) return NotFound();

                if (dto.ReportType != null) report.ReportType = dto.ReportType;
                if (dto.Title != null) report.Title = dto.Title;
                if (dto.Description != null) report.Description = dto.Description;
                if (dto.Location != null) report.Location = dto.Location;
                if (dto.ReporterName != null) report.ReporterName = dto.ReporterName;
                if (dto.ReporterContact != null) report.ReporterContact = dto.ReporterContact;
                if (dto.Status != null) report.Status = dto.Status;
                if (dto.Resolution != null) report.Resolution = dto.Resolution;
                if (dto.ResolvedDate.HasValue) report.ResolvedDate = dto.ResolvedDate;
                if (dto.AssignedTo != null) report.AssignedTo = dto.AssignedTo;
                report.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return Ok(report);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the report", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var report = await _context.CitizenReports.FindAsync(id);
            if (report == null) return NotFound();

            _context.CitizenReports.Remove(report);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}




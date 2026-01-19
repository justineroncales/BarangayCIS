using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BarangayCIS.API.Services;
using BarangayCIS.API.Models;

namespace BarangayCIS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class IncidentsController : ControllerBase
    {
        private readonly IIncidentService _incidentService;

        public IncidentsController(IIncidentService incidentService)
        {
            _incidentService = incidentService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? type, [FromQuery] string? status)
        {
            var incidents = await _incidentService.GetAllIncidentsAsync(type, status);
            return Ok(incidents);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var incident = await _incidentService.GetIncidentByIdAsync(id);
            if (incident == null) return NotFound();
            return Ok(incident);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateIncidentDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Convert DTO to Entity
                var incident = new Incident
                {
                    IncidentType = dto.IncidentType,
                    IncidentDate = dto.IncidentDate,
                    Title = dto.Title,
                    Description = dto.Description,
                    Location = dto.Location,
                    ComplainantId = dto.ComplainantId,
                    RespondentId = dto.RespondentId,
                    ComplainantName = dto.ComplainantName,
                    RespondentName = dto.RespondentName,
                    Status = dto.Status,
                    ActionTaken = dto.ActionTaken,
                    Resolution = dto.Resolution,
                    ResolutionDate = dto.ResolutionDate,
                    MediationScheduledDate = dto.MediationScheduledDate,
                    AssignedTo = dto.AssignedTo,
                    ReportedBy = dto.ReportedBy
                };

                var created = await _incidentService.CreateIncidentAsync(incident);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
            {
                var errorMessage = ex.InnerException?.Message ?? ex.Message;
                return BadRequest(new { 
                    message = "Database error while creating incident", 
                    error = errorMessage
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    message = "An error occurred while creating the incident", 
                    error = ex.Message
                });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateIncidentDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Get existing incident
            var existing = await _incidentService.GetIncidentByIdAsync(id);
            if (existing == null) return NotFound();

            // Update only provided fields
            if (dto.IncidentType != null) existing.IncidentType = dto.IncidentType;
            if (dto.IncidentDate.HasValue) existing.IncidentDate = dto.IncidentDate.Value;
            if (dto.Title != null) existing.Title = dto.Title;
            if (dto.Description != null) existing.Description = dto.Description;
            if (dto.Location != null) existing.Location = dto.Location;
            if (dto.ComplainantId.HasValue) existing.ComplainantId = dto.ComplainantId;
            if (dto.RespondentId.HasValue) existing.RespondentId = dto.RespondentId;
            if (dto.ComplainantName != null) existing.ComplainantName = dto.ComplainantName;
            if (dto.RespondentName != null) existing.RespondentName = dto.RespondentName;
            if (dto.Status != null) existing.Status = dto.Status;
            if (dto.ActionTaken != null) existing.ActionTaken = dto.ActionTaken;
            if (dto.Resolution != null) existing.Resolution = dto.Resolution;
            if (dto.ResolutionDate.HasValue) existing.ResolutionDate = dto.ResolutionDate;
            if (dto.MediationScheduledDate.HasValue) existing.MediationScheduledDate = dto.MediationScheduledDate;
            if (dto.AssignedTo != null) existing.AssignedTo = dto.AssignedTo;
            if (dto.ReportedBy != null) existing.ReportedBy = dto.ReportedBy;

            var updated = await _incidentService.UpdateIncidentAsync(id, existing);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _incidentService.DeleteIncidentAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }
    }
}



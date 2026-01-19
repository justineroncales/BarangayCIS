using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BarangayCIS.API.Data;
using BarangayCIS.API.Models;

namespace BarangayCIS.API.Controllers
{
    [ApiController]
    [Route("api/senior-citizen-ids")]
    [Authorize]
    public class SeniorCitizenIDsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SeniorCitizenIDsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? status, [FromQuery] string? search)
        {
            var query = _context.SeniorCitizenIDs
                .Include(s => s.Resident)
                .Include(s => s.Benefits)
                .Include(s => s.HealthMonitorings)
                .AsQueryable();
            
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(s => s.Status == status);
            }
            
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(s => 
                    s.SeniorCitizenNumber.Contains(search) ||
                    s.Resident.FirstName.Contains(search) ||
                    s.Resident.LastName.Contains(search));
            }
            
            var ids = await query.OrderByDescending(s => s.ApplicationDate).ToListAsync();
            return Ok(ids);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var seniorId = await _context.SeniorCitizenIDs
                .Include(s => s.Resident)
                .Include(s => s.Benefits)
                .Include(s => s.HealthMonitorings)
                .FirstOrDefaultAsync(s => s.Id == id);
            
            if (seniorId == null) return NotFound();
            return Ok(seniorId);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateSeniorCitizenIDDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var resident = await _context.Residents.FindAsync(dto.ResidentId);
                if (resident == null)
                {
                    return BadRequest(new { message = "Resident not found" });
                }

                // Check if resident is already 60 years old
                var age = DateTime.UtcNow.Year - resident.DateOfBirth.Year;
                if (DateTime.UtcNow < resident.DateOfBirth.AddYears(age)) age--;
                
                if (age < 60)
                {
                    return BadRequest(new { message = "Resident must be at least 60 years old to apply for Senior Citizen ID" });
                }

                // Generate unique Senior Citizen number
                var scNumber = $"SC-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 6).ToUpper()}";
                
                var seniorId = new SeniorCitizenID
                {
                    ResidentId = dto.ResidentId,
                    SeniorCitizenNumber = scNumber,
                    ApplicationDate = dto.ApplicationDate,
                    IssueDate = dto.IssueDate,
                    ExpiryDate = dto.ExpiryDate,
                    Status = dto.Status,
                    RequirementsSubmitted = dto.RequirementsSubmitted,
                    RequirementsMissing = dto.RequirementsMissing,
                    Remarks = dto.Remarks,
                    ProcessedBy = dto.ProcessedBy,
                    LastValidatedDate = dto.LastValidatedDate,
                    NextValidationDate = dto.NextValidationDate,
                    CreatedAt = DateTime.UtcNow
                };

                _context.SeniorCitizenIDs.Add(seniorId);
                
                // Update resident's IsSenior flag
                resident.IsSenior = true;
                resident.UpdatedAt = DateTime.UtcNow;
                
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(GetById), new { id = seniorId.Id }, seniorId);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the Senior Citizen ID", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateSeniorCitizenIDDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var seniorId = await _context.SeniorCitizenIDs.FindAsync(id);
                if (seniorId == null) return NotFound();

                seniorId.IssueDate = dto.IssueDate;
                seniorId.ExpiryDate = dto.ExpiryDate;
                seniorId.Status = dto.Status;
                seniorId.RequirementsSubmitted = dto.RequirementsSubmitted;
                seniorId.RequirementsMissing = dto.RequirementsMissing;
                seniorId.Remarks = dto.Remarks;
                seniorId.ProcessedBy = dto.ProcessedBy;
                seniorId.LastValidatedDate = dto.LastValidatedDate;
                seniorId.NextValidationDate = dto.NextValidationDate;
                seniorId.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return Ok(seniorId);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the Senior Citizen ID", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var seniorId = await _context.SeniorCitizenIDs.FindAsync(id);
            if (seniorId == null) return NotFound();

            // Update resident's IsSenior flag if this is the only SC ID
            var resident = await _context.Residents.FindAsync(seniorId.ResidentId);
            if (resident != null)
            {
                var otherScIds = await _context.SeniorCitizenIDs
                    .Where(s => s.ResidentId == resident.Id && s.Id != id)
                    .CountAsync();
                
                if (otherScIds == 0)
                {
                    resident.IsSenior = false;
                    resident.UpdatedAt = DateTime.UtcNow;
                }
            }

            _context.SeniorCitizenIDs.Remove(seniorId);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }

    [ApiController]
    [Route("api/senior-citizen-benefits")]
    [Authorize]
    public class SeniorCitizenBenefitsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SeniorCitizenBenefitsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int? seniorCitizenIDId, [FromQuery] string? benefitType, [FromQuery] string? status)
        {
            var query = _context.SeniorCitizenBenefits
                .Include(b => b.SeniorCitizenID)
                .ThenInclude(s => s.Resident)
                .AsQueryable();
            
            if (seniorCitizenIDId.HasValue)
            {
                query = query.Where(b => b.SeniorCitizenIDId == seniorCitizenIDId.Value);
            }
            
            if (!string.IsNullOrEmpty(benefitType))
            {
                query = query.Where(b => b.BenefitType == benefitType);
            }
            
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(b => b.Status == status);
            }
            
            var benefits = await query.OrderByDescending(b => b.BenefitDate).ToListAsync();
            return Ok(benefits);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var benefit = await _context.SeniorCitizenBenefits
                .Include(b => b.SeniorCitizenID)
                .ThenInclude(s => s.Resident)
                .FirstOrDefaultAsync(b => b.Id == id);
            
            if (benefit == null) return NotFound();
            return Ok(benefit);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateSeniorCitizenBenefitDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var seniorId = await _context.SeniorCitizenIDs.FindAsync(dto.SeniorCitizenIDId);
                if (seniorId == null)
                {
                    return BadRequest(new { message = "Senior Citizen ID not found" });
                }

                var benefit = new SeniorCitizenBenefit
                {
                    SeniorCitizenIDId = dto.SeniorCitizenIDId,
                    BenefitType = dto.BenefitType,
                    BenefitDescription = dto.BenefitDescription,
                    Amount = dto.Amount,
                    BenefitDate = dto.BenefitDate,
                    Status = dto.Status,
                    Requirements = dto.Requirements,
                    Notes = dto.Notes,
                    ProcessedBy = dto.ProcessedBy,
                    ProcessedDate = dto.ProcessedDate,
                    ReferenceNumber = dto.ReferenceNumber,
                    PaymentMethod = dto.PaymentMethod,
                    CreatedAt = DateTime.UtcNow
                };

                _context.SeniorCitizenBenefits.Add(benefit);
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(GetById), new { id = benefit.Id }, benefit);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the benefit", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateSeniorCitizenBenefitDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var benefit = await _context.SeniorCitizenBenefits.FindAsync(id);
                if (benefit == null) return NotFound();

                benefit.BenefitType = dto.BenefitType;
                benefit.BenefitDescription = dto.BenefitDescription;
                benefit.Amount = dto.Amount;
                benefit.BenefitDate = dto.BenefitDate;
                benefit.Status = dto.Status;
                benefit.Requirements = dto.Requirements;
                benefit.Notes = dto.Notes;
                benefit.ProcessedBy = dto.ProcessedBy;
                benefit.ProcessedDate = dto.ProcessedDate;
                benefit.ReferenceNumber = dto.ReferenceNumber;
                benefit.PaymentMethod = dto.PaymentMethod;
                benefit.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return Ok(benefit);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the benefit", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var benefit = await _context.SeniorCitizenBenefits.FindAsync(id);
            if (benefit == null) return NotFound();

            _context.SeniorCitizenBenefits.Remove(benefit);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }

    [ApiController]
    [Route("api/senior-health-monitorings")]
    [Authorize]
    public class SeniorHealthMonitoringsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SeniorHealthMonitoringsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int? seniorCitizenIDId, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            var query = _context.SeniorHealthMonitorings
                .Include(m => m.SeniorCitizenID)
                .ThenInclude(s => s.Resident)
                .AsQueryable();
            
            if (seniorCitizenIDId.HasValue)
            {
                query = query.Where(m => m.SeniorCitizenIDId == seniorCitizenIDId.Value);
            }
            
            if (fromDate.HasValue)
            {
                query = query.Where(m => m.MonitoringDate >= fromDate.Value);
            }
            
            if (toDate.HasValue)
            {
                query = query.Where(m => m.MonitoringDate <= toDate.Value);
            }
            
            var monitorings = await query.OrderByDescending(m => m.MonitoringDate).ToListAsync();
            return Ok(monitorings);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var monitoring = await _context.SeniorHealthMonitorings
                .Include(m => m.SeniorCitizenID)
                .ThenInclude(s => s.Resident)
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (monitoring == null) return NotFound();
            return Ok(monitoring);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateSeniorHealthMonitoringDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var seniorId = await _context.SeniorCitizenIDs.FindAsync(dto.SeniorCitizenIDId);
                if (seniorId == null)
                {
                    return BadRequest(new { message = "Senior Citizen ID not found" });
                }

                var monitoring = new SeniorHealthMonitoring
                {
                    SeniorCitizenIDId = dto.SeniorCitizenIDId,
                    MonitoringDate = dto.MonitoringDate,
                    MonitoringType = dto.MonitoringType,
                    BloodPressure = dto.BloodPressure,
                    BloodSugar = dto.BloodSugar,
                    Weight = dto.Weight,
                    Height = dto.Height,
                    BMI = dto.BMI,
                    HealthFindings = dto.HealthFindings,
                    Complaints = dto.Complaints,
                    Medications = dto.Medications,
                    Recommendations = dto.Recommendations,
                    ReferralStatus = dto.ReferralStatus,
                    ReferralNotes = dto.ReferralNotes,
                    AttendedBy = dto.AttendedBy,
                    Notes = dto.Notes,
                    NextCheckupDate = dto.NextCheckupDate,
                    CreatedAt = DateTime.UtcNow
                };

                _context.SeniorHealthMonitorings.Add(monitoring);
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(GetById), new { id = monitoring.Id }, monitoring);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the health monitoring", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateSeniorHealthMonitoringDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var monitoring = await _context.SeniorHealthMonitorings.FindAsync(id);
                if (monitoring == null) return NotFound();

                monitoring.MonitoringDate = dto.MonitoringDate;
                monitoring.MonitoringType = dto.MonitoringType;
                monitoring.BloodPressure = dto.BloodPressure;
                monitoring.BloodSugar = dto.BloodSugar;
                monitoring.Weight = dto.Weight;
                monitoring.Height = dto.Height;
                monitoring.BMI = dto.BMI;
                monitoring.HealthFindings = dto.HealthFindings;
                monitoring.Complaints = dto.Complaints;
                monitoring.Medications = dto.Medications;
                monitoring.Recommendations = dto.Recommendations;
                monitoring.ReferralStatus = dto.ReferralStatus;
                monitoring.ReferralNotes = dto.ReferralNotes;
                monitoring.AttendedBy = dto.AttendedBy;
                monitoring.Notes = dto.Notes;
                monitoring.NextCheckupDate = dto.NextCheckupDate;

                await _context.SaveChangesAsync();
                return Ok(monitoring);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the health monitoring", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var monitoring = await _context.SeniorHealthMonitorings.FindAsync(id);
            if (monitoring == null) return NotFound();

            _context.SeniorHealthMonitorings.Remove(monitoring);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}


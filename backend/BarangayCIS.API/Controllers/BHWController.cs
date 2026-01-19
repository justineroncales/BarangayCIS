using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BarangayCIS.API.Data;
using BarangayCIS.API.Models;
using System.Text;

namespace BarangayCIS.API.Controllers
{
    [ApiController]
    [Route("api/bhw-profiles")]
    [Authorize]
    public class BHWProfilesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public BHWProfilesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? status, [FromQuery] string? search)
        {
            var query = _context.BHWProfiles.Include(b => b.Resident).AsQueryable();
            
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(b => b.Status == status);
            }
            
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(b => 
                    b.FirstName.Contains(search) || 
                    b.LastName.Contains(search) || 
                    b.BHWNumber.Contains(search));
            }
            
            var profiles = await query.OrderBy(b => b.LastName).ThenBy(b => b.FirstName).ToListAsync();
            return Ok(profiles);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var profile = await _context.BHWProfiles
                .Include(b => b.Resident)
                .Include(b => b.Assignments)
                .Include(b => b.VisitLogs)
                .Include(b => b.Trainings)
                .Include(b => b.Incentives)
                .FirstOrDefaultAsync(b => b.Id == id);
            
            if (profile == null) return NotFound();
            return Ok(profile);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateBHWProfileDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Generate unique BHW number
                var bhwNumber = $"BHW-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 6).ToUpper()}";
                
                var profile = new BHWProfile
                {
                    ResidentId = dto.ResidentId,
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    MiddleName = dto.MiddleName,
                    Suffix = dto.Suffix,
                    DateOfBirth = dto.DateOfBirth,
                    Gender = dto.Gender,
                    Address = dto.Address,
                    ContactNumber = dto.ContactNumber,
                    Email = dto.Email,
                    CivilStatus = dto.CivilStatus,
                    EducationalAttainment = dto.EducationalAttainment,
                    BHWNumber = bhwNumber,
                    DateAppointed = dto.DateAppointed,
                    Status = dto.Status,
                    Specialization = dto.Specialization,
                    Notes = dto.Notes,
                    CreatedAt = DateTime.UtcNow
                };

                _context.BHWProfiles.Add(profile);
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(GetById), new { id = profile.Id }, profile);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the BHW profile", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateBHWProfileDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var profile = await _context.BHWProfiles.FindAsync(id);
                if (profile == null) return NotFound();

                if (dto.ResidentId.HasValue)
                {
                    var resident = await _context.Residents.FindAsync(dto.ResidentId.Value);
                    if (resident == null)
                    {
                        return BadRequest(new { message = "Resident not found" });
                    }
                    profile.ResidentId = dto.ResidentId;
                }

                profile.FirstName = dto.FirstName;
                profile.LastName = dto.LastName;
                profile.MiddleName = dto.MiddleName;
                profile.Suffix = dto.Suffix;
                profile.DateOfBirth = dto.DateOfBirth;
                profile.Gender = dto.Gender;
                profile.Address = dto.Address;
                profile.ContactNumber = dto.ContactNumber;
                profile.Email = dto.Email;
                profile.CivilStatus = dto.CivilStatus;
                profile.EducationalAttainment = dto.EducationalAttainment;
                profile.DateAppointed = dto.DateAppointed;
                profile.Status = dto.Status;
                profile.Specialization = dto.Specialization;
                profile.Notes = dto.Notes;
                profile.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return Ok(profile);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the BHW profile", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var profile = await _context.BHWProfiles.FindAsync(id);
            if (profile == null) return NotFound();

            _context.BHWProfiles.Remove(profile);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("population-age-distribution/print")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPopulationAgeDistributionReport([FromQuery] int? bhwProfileId, [FromQuery] string? quarter, [FromQuery] int? year, [FromQuery] string? submittedBy)
        {
            try
            {
                // Get residents - filter by BHW if specified
                var residentsQuery = _context.Residents.AsQueryable();
                
                if (bhwProfileId.HasValue)
                {
                    residentsQuery = residentsQuery.Where(r => r.BHWProfileId == bhwProfileId.Value);
                }

                var residents = await residentsQuery.ToListAsync();

                // Calculate age groups
                var ageDistribution = CalculateAgeDistribution(residents);

                // Get BHW info if filtering by specific BHW
                BHWProfile? bhwProfile = null;
                if (bhwProfileId.HasValue)
                {
                    bhwProfile = await _context.BHWProfiles.FindAsync(bhwProfileId.Value);
                }

                // Generate HTML report
                var html = GeneratePopulationAgeDistributionHtml(ageDistribution, bhwProfile, quarter ?? "4TH", year ?? DateTime.Now.Year, submittedBy);

                return Content(html, "text/html", Encoding.UTF8);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while generating the report", error = ex.Message });
            }
        }

        private List<AgeGroupData> CalculateAgeDistribution(List<Resident> residents)
        {
            var now = DateTime.Now;
            var ageGroups = new List<AgeGroupData>();

            // Define age groups based on the image
            var groups = new[]
            {
                new { Label = "UNDER 1", MinAge = 0, MaxAge = 0 },
                new { Label = "1-4", MinAge = 1, MaxAge = 4 },
                new { Label = "5-9", MinAge = 5, MaxAge = 9 },
                new { Label = "10-14", MinAge = 10, MaxAge = 14 },
                new { Label = "15-19", MinAge = 15, MaxAge = 19 },
                new { Label = "20-24", MinAge = 20, MaxAge = 24 },
                new { Label = "25-29", MinAge = 25, MaxAge = 29 },
                new { Label = "30-34", MinAge = 30, MaxAge = 34 },
                new { Label = "35-39", MinAge = 35, MaxAge = 39 },
                new { Label = "40-44", MinAge = 40, MaxAge = 44 },
                new { Label = "45-49", MinAge = 45, MaxAge = 49 },
                new { Label = "50-54", MinAge = 50, MaxAge = 54 },
                new { Label = "55-59", MinAge = 55, MaxAge = 59 },
                new { Label = "60-64", MinAge = 60, MaxAge = 64 },
                new { Label = "65-69", MinAge = 65, MaxAge = 69 },
                new { Label = "70-74", MinAge = 70, MaxAge = 74 },
                new { Label = "75-79", MinAge = 75, MaxAge = 79 },
                new { Label = "80-OVER", MinAge = 80, MaxAge = 999 }
            };

            foreach (var group in groups)
            {
                var residentsInGroup = residents.Where(r =>
                {
                    var age = CalculateAge(r.DateOfBirth, now);
                    if (group.Label == "UNDER 1")
                    {
                        return age < 1;
                    }
                    else if (group.Label == "80-OVER")
                    {
                        return age >= 80;
                    }
                    else
                    {
                        return age >= group.MinAge && age <= group.MaxAge;
                    }
                }).ToList();

                var maleCount = residentsInGroup.Count(r => r.Gender?.ToUpper() == "MALE" || r.Gender?.ToUpper() == "M");
                var femaleCount = residentsInGroup.Count(r => r.Gender?.ToUpper() == "FEMALE" || r.Gender?.ToUpper() == "F");
                var total = residentsInGroup.Count;

                ageGroups.Add(new AgeGroupData
                {
                    AgeGroup = group.Label,
                    Male = maleCount,
                    Female = femaleCount,
                    Total = total
                });
            }

            // Add totals
            var totalMale = ageGroups.Sum(g => g.Male);
            var totalFemale = ageGroups.Sum(g => g.Female);
            var grandTotal = ageGroups.Sum(g => g.Total);

            ageGroups.Add(new AgeGroupData
            {
                AgeGroup = "TOTAL",
                Male = totalMale,
                Female = totalFemale,
                Total = grandTotal
            });

            return ageGroups;
        }

        private int CalculateAge(DateTime dateOfBirth, DateTime referenceDate)
        {
            var age = referenceDate.Year - dateOfBirth.Year;
            if (referenceDate.Month < dateOfBirth.Month || (referenceDate.Month == dateOfBirth.Month && referenceDate.Day < dateOfBirth.Day))
            {
                age--;
            }
            return age;
        }

        private string GeneratePopulationAgeDistributionHtml(List<AgeGroupData> ageDistribution, BHWProfile? bhwProfile, string quarter, int year, string? submittedBy)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html>");
            sb.AppendLine("<head>");
            sb.AppendLine("<meta charset='UTF-8'>");
            sb.AppendLine("<title>POP AGE DISTRIBUTION</title>");
            sb.AppendLine("<style>");
            sb.AppendLine("@page { margin: 0.5in; size: legal; }");
            sb.AppendLine("@media print { @page { size: 8.5in 14in; margin: 0.5in; } body { width: 100% !important; max-width: 100% !important; margin: 0 !important; padding: 0.5in !important; box-shadow: none !important; } }");
            sb.AppendLine("* { margin: 0; padding: 0; box-sizing: border-box; }");
            sb.AppendLine("body { font-family: 'Times New Roman', serif; font-size: 12pt; margin: 20px auto; padding: 20px; background: white; }");
            sb.AppendLine(".report-header { text-align: center; margin-bottom: 20px; }");
            sb.AppendLine(".report-title { font-size: 16pt; font-weight: bold; text-decoration: underline; margin-bottom: 10px; }");
            sb.AppendLine(".report-subtitle { font-size: 12pt; margin-bottom: 5px; }");
            sb.AppendLine(".report-table { width: 100%; border-collapse: collapse; margin: 20px 0; }");
            sb.AppendLine(".report-table th, .report-table td { border: 1px solid #000; padding: 8px; text-align: center; }");
            sb.AppendLine(".report-table th { background-color: #f0f0f0; font-weight: bold; }");
            sb.AppendLine(".report-table .age-group { text-align: left; padding-left: 10px; }");
            sb.AppendLine(".report-table .total-row { font-weight: bold; background-color: #e0e0e0; }");
            sb.AppendLine(".report-footer { margin-top: 30px; display: flex; justify-content: space-between; }");
            sb.AppendLine(".report-footer .date-section { text-align: left; }");
            sb.AppendLine(".report-footer .submitted-section { text-align: right; }");
            sb.AppendLine(".report-footer .date-value { text-decoration: underline; }");
            sb.AppendLine("</style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");

            // Header
            sb.AppendLine("<div class='report-header'>");
            sb.AppendLine("<div class='report-title'>POP AGE DISTRIBUTION</div>");
            sb.AppendLine($"<div class='report-subtitle'>{year} QUARTER {quarter}</div>");
            sb.AppendLine("<div class='report-subtitle'>BARANGAY: " + (bhwProfile != null ? "PDB-5" : "PDB-5") + "</div>");
            sb.AppendLine("</div>");

            // Table
            sb.AppendLine("<table class='report-table'>");
            sb.AppendLine("<thead>");
            sb.AppendLine("<tr>");
            sb.AppendLine("<th>AGE GROUP</th>");
            sb.AppendLine("<th>MALE</th>");
            sb.AppendLine("<th>FEMALE</th>");
            sb.AppendLine("<th>TOTAL</th>");
            sb.AppendLine("</tr>");
            sb.AppendLine("</thead>");
            sb.AppendLine("<tbody>");

            foreach (var group in ageDistribution)
            {
                var isTotal = group.AgeGroup == "TOTAL";
                var rowClass = isTotal ? "total-row" : "";
                sb.AppendLine($"<tr class='{rowClass}'>");
                sb.AppendLine($"<td class='age-group'>{group.AgeGroup}</td>");
                sb.AppendLine($"<td>{group.Male}</td>");
                sb.AppendLine($"<td>{group.Female}</td>");
                sb.AppendLine($"<td>{group.Total}</td>");
                sb.AppendLine("</tr>");
            }

            sb.AppendLine("</tbody>");
            sb.AppendLine("</table>");

            // Footer
            var currentDate = DateTime.Now.ToString("MM-dd-yy");
            sb.AppendLine("<div class='report-footer'>");
            sb.AppendLine("<div class='date-section'>");
            sb.AppendLine($"<div class='date-value'>{currentDate}</div>");
            sb.AppendLine("<div>Date</div>");
            sb.AppendLine("</div>");
            sb.AppendLine("<div class='submitted-section'>");
            sb.AppendLine("<div>Submitted By:</div>");
            if (!string.IsNullOrEmpty(submittedBy))
            {
                sb.AppendLine($"<div><strong>{submittedBy.ToUpper()}</strong></div>");
                sb.AppendLine("<div>BHW</div>");
            }
            else if (bhwProfile != null)
            {
                sb.AppendLine($"<div><strong>{bhwProfile.FirstName.ToUpper()} {bhwProfile.MiddleName?.ToUpper()} {bhwProfile.LastName.ToUpper()}</strong></div>");
                sb.AppendLine("<div>BHW</div>");
            }
            else
            {
                sb.AppendLine("<div>_____________</div>");
                sb.AppendLine("<div>BHW</div>");
            }
            sb.AppendLine("</div>");
            sb.AppendLine("</div>");

            // Auto-print script
            sb.AppendLine("<script>");
            sb.AppendLine("(function() {");
            sb.AppendLine("  function triggerPrint() {");
            sb.AppendLine("    window.print();");
            sb.AppendLine("  }");
            sb.AppendLine("  if (document.readyState === 'complete') {");
            sb.AppendLine("    setTimeout(triggerPrint, 500);");
            sb.AppendLine("  } else {");
            sb.AppendLine("    window.addEventListener('load', function() {");
            sb.AppendLine("      setTimeout(triggerPrint, 500);");
            sb.AppendLine("    });");
            sb.AppendLine("  }");
            sb.AppendLine("})();");
            sb.AppendLine("</script>");

            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            return sb.ToString();
        }

        [HttpGet("annual-catchment-population/print")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAnnualCatchmentPopulationReport([FromQuery] int? bhwProfileId, [FromQuery] int? year, [FromQuery] string? submittedBy)
        {
            try
            {
                // Get residents - filter by BHW if specified
                var residentsQuery = _context.Residents.AsQueryable();
                
                if (bhwProfileId.HasValue)
                {
                    residentsQuery = residentsQuery.Where(r => r.BHWProfileId == bhwProfileId.Value);
                }

                var residents = await residentsQuery.ToListAsync();

                // Calculate age groups for Annual Catchment Population Summary
                var catchmentData = CalculateCatchmentAgeDistribution(residents);

                // Get BHW info if filtering by specific BHW
                BHWProfile? bhwProfile = null;
                if (bhwProfileId.HasValue)
                {
                    bhwProfile = await _context.BHWProfiles.FindAsync(bhwProfileId.Value);
                }

                // Generate HTML report
                var html = GenerateAnnualCatchmentPopulationHtml(catchmentData, bhwProfile, year ?? DateTime.Now.Year, submittedBy);

                return Content(html, "text/html", Encoding.UTF8);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while generating the report", error = ex.Message });
            }
        }

        [HttpGet("pop-age-individual/print")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPopAgeIndividualReport([FromQuery] int? bhwProfileId, [FromQuery] int? year, [FromQuery] string? submittedBy)
        {
            try
            {
                // Get residents - filter by BHW if specified
                var residentsQuery = _context.Residents.AsQueryable();
                
                if (bhwProfileId.HasValue)
                {
                    residentsQuery = residentsQuery.Where(r => r.BHWProfileId == bhwProfileId.Value);
                }

                var residents = await residentsQuery.ToListAsync();

                // Calculate individual age distribution
                var individualAgeData = CalculateIndividualAgeDistribution(residents);

                // Get BHW info if filtering by specific BHW
                BHWProfile? bhwProfile = null;
                if (bhwProfileId.HasValue)
                {
                    bhwProfile = await _context.BHWProfiles.FindAsync(bhwProfileId.Value);
                }

                // Generate HTML report
                var html = GeneratePopAgeIndividualHtml(individualAgeData, bhwProfile, year ?? DateTime.Now.Year, submittedBy);

                return Content(html, "text/html", Encoding.UTF8);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while generating the report", error = ex.Message });
            }
        }

        [HttpGet("wra-report/print")]
        [AllowAnonymous]
        public async Task<IActionResult> GetWRAReport([FromQuery] int? bhwProfileId, [FromQuery] int? month, [FromQuery] int? year, [FromQuery] string? submittedBy)
        {
            try
            {
                // Get residents - filter by BHW if specified
                var residentsQuery = _context.Residents.AsQueryable();
                
                if (bhwProfileId.HasValue)
                {
                    residentsQuery = residentsQuery.Where(r => r.BHWProfileId == bhwProfileId.Value);
                }

                // Filter by gender (Female) and age range (10-49 years)
                var residents = await residentsQuery.ToListAsync();
                var reportDate = new DateTime(year ?? DateTime.Now.Year, month ?? DateTime.Now.Month, 1);

                // Calculate WRA distribution
                var wraData = CalculateWRADistribution(residents, reportDate);

                // Get BHW info if filtering by specific BHW
                BHWProfile? bhwProfile = null;
                if (bhwProfileId.HasValue)
                {
                    bhwProfile = await _context.BHWProfiles.FindAsync(bhwProfileId.Value);
                }

                // Generate HTML report
                var html = GenerateWRAReportHtml(wraData, bhwProfile, month ?? DateTime.Now.Month, year ?? DateTime.Now.Year, submittedBy);

                return Content(html, "text/html", Encoding.UTF8);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while generating the WRA report", error = ex.Message });
            }
        }

        private WRADistributionData CalculateWRADistribution(List<Resident> residents, DateTime referenceDate)
        {
            var wraData = new WRADistributionData();

            foreach (var resident in residents)
            {
                // Only count females
                if (resident.Gender?.ToUpper() != "FEMALE" && resident.Gender?.ToUpper() != "F")
                    continue;

                var age = CalculateAge(resident.DateOfBirth, referenceDate);

                // WRA is typically 15-49, but form includes 10-14
                if (age >= 10 && age <= 49)
                {
                    if (age >= 10 && age <= 14)
                    {
                        wraData.Age10To14++;
                    }
                    else if (age >= 15 && age <= 19)
                    {
                        wraData.Age15To19++;
                    }
                    else if (age >= 20 && age <= 49)
                    {
                        wraData.Age20To49++;
                    }
                }
            }

            wraData.TotalWRA15To49 = wraData.Age15To19 + wraData.Age20To49;
            wraData.TotalWRA = wraData.Age10To14 + wraData.Age15To19 + wraData.Age20To49;

            return wraData;
        }

        private string GenerateWRAReportHtml(WRADistributionData wraData, BHWProfile? bhwProfile, int month, int year, string? submittedBy)
        {
            var monthNames = new[] { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };
            var monthName = monthNames[Math.Max(0, Math.Min(month - 1, 11))];

            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html>");
            sb.AppendLine("<head>");
            sb.AppendLine("<meta charset='UTF-8'>");
            sb.AppendLine("<title>FHSIS REPORT - WRA</title>");
            sb.AppendLine("<style>");
            sb.AppendLine("@page { margin: 0.5in; size: legal landscape; }");
            sb.AppendLine("@media print { @page { size: 11in 8.5in; margin: 0.5in; } body { width: 100% !important; max-width: 100% !important; margin: 0 !important; padding: 0.5in !important; box-shadow: none !important; } }");
            sb.AppendLine("* { margin: 0; padding: 0; box-sizing: border-box; }");
            sb.AppendLine("body { font-family: 'Times New Roman', serif; font-size: 10pt; margin: 20px auto; padding: 20px; background: white; }");
            sb.AppendLine(".report-header { text-align: center; margin-bottom: 15px; }");
            sb.AppendLine(".doh-header { font-size: 10pt; margin-bottom: 5px; }");
            sb.AppendLine(".report-title { font-size: 12pt; font-weight: bold; margin-bottom: 5px; }");
            sb.AppendLine(".report-subtitle { font-size: 10pt; margin-bottom: 3px; }");
            sb.AppendLine(".form-fields { display: grid; grid-template-columns: repeat(2, 1fr); gap: 10px; margin-bottom: 15px; font-size: 9pt; }");
            sb.AppendLine(".form-field { display: flex; }");
            sb.AppendLine(".form-field label { font-weight: bold; min-width: 120px; }");
            sb.AppendLine(".form-field span { flex: 1; border-bottom: 1px solid #000; padding-left: 5px; }");
            sb.AppendLine(".report-table { width: 100%; border-collapse: collapse; margin: 15px 0; font-size: 9pt; }");
            sb.AppendLine(".report-table th, .report-table td { border: 1px solid #000; padding: 5px; text-align: center; }");
            sb.AppendLine(".report-table th { background-color: #f0f0f0; font-weight: bold; }");
            sb.AppendLine(".report-table .text-left { text-align: left; padding-left: 8px; }");
            sb.AppendLine(".report-table .age-header { writing-mode: vertical-rl; text-orientation: mixed; height: 80px; }");
            sb.AppendLine("</style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");

            // Header
            sb.AppendLine("<div class='report-header'>");
            sb.AppendLine("<div class='doh-header'>REPUBLIC OF THE PHILIPPINES</div>");
            sb.AppendLine("<div class='doh-header'>DEPARTMENT OF HEALTH</div>");
            sb.AppendLine("<div class='report-title'>FHSIS REPORT</div>");
            sb.AppendLine("<div class='report-subtitle'>M1 - No. of WRA w/ unmet need for modern FP-Total</div>");
            sb.AppendLine("</div>");

            // Form Fields
            sb.AppendLine("<div class='form-fields'>");
            sb.AppendLine("<div class='form-field'><label>Name of Barangay:</label><span>PDB-5</span></div>");
            sb.AppendLine("<div class='form-field'><label>Name of BHS:</label><span>BARANGAY 5 HEALTH STATION</span></div>");
            sb.AppendLine("<div class='form-field'><label>Name of Municipality/City:</label><span>LIAN</span></div>");
            sb.AppendLine("<div class='form-field'><label>Name of Province:</label><span>BATANGAS</span></div>");
            sb.AppendLine("<div class='form-field'><label>Projected Population:</label><span></span></div>");
            sb.AppendLine($"<div class='form-field'><label>Month:</label><span>{monthName}</span></div>");
            sb.AppendLine($"<div class='form-field'><label>Year:</label><span>{year}</span></div>");
            sb.AppendLine("<div class='form-field'><label style='font-size: 8pt;'>For submission to RHU/MHC:</label><span></span></div>");
            sb.AppendLine("</div>");

            // Table
            sb.AppendLine("<table class='report-table'>");
            sb.AppendLine("<thead>");
            sb.AppendLine("<tr>");
            sb.AppendLine("<th rowspan='2' style='width: 120px;'>No. of WRA with unmet need for modern FP - Total</th>");
            sb.AppendLine("<th colspan='3'>AGE</th>");
            sb.AppendLine("<th rowspan='2' style='width: 100px;'>Total for WRA 15-49 y/o</th>");
            sb.AppendLine("<th rowspan='2' style='width: 100px;'>Remarks</th>");
            sb.AppendLine("</tr>");
            sb.AppendLine("<tr>");
            sb.AppendLine("<th style='width: 80px;'>10-14 y/o</th>");
            sb.AppendLine("<th style='width: 80px;'>15-19 y/o</th>");
            sb.AppendLine("<th style='width: 80px;'>20-49 y/o</th>");
            sb.AppendLine("</tr>");
            sb.AppendLine("</thead>");
            sb.AppendLine("<tbody>");
            
            // Data row
            sb.AppendLine("<tr>");
            sb.AppendLine($"<td class='text-left'>{wraData.TotalWRA}</td>");
            sb.AppendLine($"<td>{wraData.Age10To14}</td>");
            sb.AppendLine($"<td>{wraData.Age15To19}</td>");
            sb.AppendLine($"<td>{wraData.Age20To49}</td>");
            sb.AppendLine($"<td>{wraData.TotalWRA15To49}</td>");
            sb.AppendLine("<td></td>");
            sb.AppendLine("</tr>");
            
            sb.AppendLine("</tbody>");
            sb.AppendLine("</table>");

            // Footer
            var currentDate = DateTime.Now.ToString("MM-dd-yy");
            sb.AppendLine("<div style='margin-top: 30px; display: flex; justify-content: space-between; font-size: 9pt;'>");
            sb.AppendLine("<div>");
            sb.AppendLine($"<div style='text-decoration: underline;'>{currentDate}</div>");
            sb.AppendLine("<div>Date</div>");
            sb.AppendLine("</div>");
            sb.AppendLine("<div style='text-align: right;'>");
            sb.AppendLine("<div>Submitted By:</div>");
            if (!string.IsNullOrEmpty(submittedBy))
            {
                sb.AppendLine($"<div><strong>{submittedBy.ToUpper()}</strong></div>");
                sb.AppendLine("<div>BHW</div>");
            }
            else if (bhwProfile != null)
            {
                sb.AppendLine($"<div><strong>{bhwProfile.FirstName.ToUpper()} {bhwProfile.MiddleName?.ToUpper()} {bhwProfile.LastName.ToUpper()}</strong></div>");
                sb.AppendLine("<div>BHW</div>");
            }
            else
            {
                sb.AppendLine("<div>_____________</div>");
                sb.AppendLine("<div>BHW</div>");
            }
            sb.AppendLine("</div>");
            sb.AppendLine("</div>");

            // Auto-print script
            sb.AppendLine("<script>");
            sb.AppendLine("(function() {");
            sb.AppendLine("  function triggerPrint() {");
            sb.AppendLine("    window.print();");
            sb.AppendLine("  }");
            sb.AppendLine("  if (document.readyState === 'complete') {");
            sb.AppendLine("    setTimeout(triggerPrint, 500);");
            sb.AppendLine("  } else {");
            sb.AppendLine("    window.addEventListener('load', function() {");
            sb.AppendLine("      setTimeout(triggerPrint, 500);");
            sb.AppendLine("    });");
            sb.AppendLine("  }");
            sb.AppendLine("})();");
            sb.AppendLine("</script>");

            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            return sb.ToString();
        }

        private class AgeGroupData
        {
            public string AgeGroup { get; set; } = string.Empty;
            public int Male { get; set; }
            public int Female { get; set; }
            public int Total { get; set; }
        }

        private List<CatchmentAgeGroupData> CalculateCatchmentAgeDistribution(List<Resident> residents)
        {
            var now = DateTime.Now;
            var ageGroups = new List<CatchmentAgeGroupData>();

            var groups = new[]
            {
                new { Label = "LESS THAN- 1 YR.", MinAge = 0, MaxAge = 0 },
                new { Label = "1-4", MinAge = 1, MaxAge = 4 },
                new { Label = "5-6", MinAge = 5, MaxAge = 6 },
                new { Label = "7-14", MinAge = 7, MaxAge = 14 },
                new { Label = "15-49", MinAge = 15, MaxAge = 49 },
                new { Label = "50-64", MinAge = 50, MaxAge = 64 },
                new { Label = "65- OVER", MinAge = 65, MaxAge = 999 }
            };

            foreach (var group in groups)
            {
                var residentsInGroup = residents.Where(r =>
                {
                    var age = CalculateAge(r.DateOfBirth, now);
                    if (group.Label == "LESS THAN- 1 YR.")
                    {
                        return age < 1;
                    }
                    else if (group.Label == "65- OVER")
                    {
                        return age >= 65;
                    }
                    else
                    {
                        return age >= group.MinAge && age <= group.MaxAge;
                    }
                }).ToList();

                var maleCount = residentsInGroup.Count(r => r.Gender?.ToUpper() == "MALE" || r.Gender?.ToUpper() == "M");
                var femaleCount = residentsInGroup.Count(r => r.Gender?.ToUpper() == "FEMALE" || r.Gender?.ToUpper() == "F");
                var total = residentsInGroup.Count;

                ageGroups.Add(new CatchmentAgeGroupData
                {
                    AgeGroup = group.Label,
                    Male = maleCount,
                    Female = femaleCount,
                    Total = total
                });
            }

            // Add totals
            var totalMale = ageGroups.Sum(g => g.Male);
            var totalFemale = ageGroups.Sum(g => g.Female);
            var grandTotal = ageGroups.Sum(g => g.Total);

            ageGroups.Add(new CatchmentAgeGroupData
            {
                AgeGroup = "TOTAL",
                Male = totalMale,
                Female = totalFemale,
                Total = grandTotal
            });

            return ageGroups;
        }

        private Dictionary<int, IndividualAgeData> CalculateIndividualAgeDistribution(List<Resident> residents)
        {
            var now = DateTime.Now;
            var ageData = new Dictionary<int, IndividualAgeData>();

            // Process each resident
            foreach (var resident in residents)
            {
                var age = CalculateAge(resident.DateOfBirth, now);
                
                // Handle special cases for infants
                if (age == 0)
                {
                    var months = (now.Year - resident.DateOfBirth.Year) * 12 + (now.Month - resident.DateOfBirth.Month);
                    if (months < 0) months = 0;
                    
                    // Categorize by months
                    if (months <= 5)
                    {
                        if (!ageData.ContainsKey(-1)) // -1 for 0-5 months
                        {
                            ageData[-1] = new IndividualAgeData { AgeLabel = "0-5 MONTHS", Male = 0, Female = 0, Total = 0 };
                        }
                        if (resident.Gender?.ToUpper() == "MALE" || resident.Gender?.ToUpper() == "M")
                            ageData[-1].Male++;
                        else
                            ageData[-1].Female++;
                        ageData[-1].Total++;
                    }
                    else // 6-11 months
                    {
                        if (!ageData.ContainsKey(-2)) // -2 for 6-11 months
                        {
                            ageData[-2] = new IndividualAgeData { AgeLabel = "6-11 MONTHS", Male = 0, Female = 0, Total = 0 };
                        }
                        if (resident.Gender?.ToUpper() == "MALE" || resident.Gender?.ToUpper() == "M")
                            ageData[-2].Male++;
                        else
                            ageData[-2].Female++;
                        ageData[-2].Total++;
                    }
                }
                else
                {
                    // Regular age grouping
                    if (!ageData.ContainsKey(age))
                    {
                        ageData[age] = new IndividualAgeData { AgeLabel = age.ToString(), Male = 0, Female = 0, Total = 0 };
                    }
                    if (resident.Gender?.ToUpper() == "MALE" || resident.Gender?.ToUpper() == "M")
                        ageData[age].Male++;
                    else
                        ageData[age].Female++;
                    ageData[age].Total++;
                }
            }

            return ageData;
        }

        private string GenerateAnnualCatchmentPopulationHtml(List<CatchmentAgeGroupData> catchmentData, BHWProfile? bhwProfile, int year, string? submittedBy)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html>");
            sb.AppendLine("<head>");
            sb.AppendLine("<meta charset='UTF-8'>");
            sb.AppendLine("<title>ANNUAL CATCHMENT POPULATION SUMMARY REPORT</title>");
            sb.AppendLine("<style>");
            sb.AppendLine("@page { margin: 0.5in; size: letter; }");
            sb.AppendLine("@media print { @page { size: 8.5in 11in; margin: 0.5in; } body { width: 100% !important; max-width: 100% !important; margin: 0 !important; padding: 0.5in !important; box-shadow: none !important; } }");
            sb.AppendLine("* { margin: 0; padding: 0; box-sizing: border-box; }");
            sb.AppendLine("body { font-family: 'Times New Roman', serif; font-size: 11pt; margin: 20px auto; padding: 20px; background: white; }");
            sb.AppendLine(".report-header { text-align: center; margin-bottom: 20px; position: relative; }");
            sb.AppendLine(".report-title { font-size: 14pt; font-weight: bold; margin-bottom: 5px; }");
            sb.AppendLine(".report-subtitle { font-size: 11pt; margin-bottom: 3px; }");
            sb.AppendLine(".report-table { width: 100%; border-collapse: collapse; margin: 20px 0; }");
            sb.AppendLine(".report-table th, .report-table td { border: 1px solid #000; padding: 6px; text-align: center; font-size: 10pt; }");
            sb.AppendLine(".report-table th { background-color: #f0f0f0; font-weight: bold; }");
            sb.AppendLine(".report-table .age-group { text-align: left; padding-left: 8px; }");
            sb.AppendLine(".report-table .total-row { font-weight: bold; background-color: #e0e0e0; }");
            sb.AppendLine(".report-footer { margin-top: 30px; display: flex; justify-content: space-between; font-size: 10pt; }");
            sb.AppendLine(".report-footer .date-section { text-align: left; }");
            sb.AppendLine(".report-footer .submitted-section { text-align: right; }");
            sb.AppendLine(".report-footer .date-value { text-decoration: underline; }");
            sb.AppendLine("</style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");

            // Header
            sb.AppendLine("<div class='report-header'>");
            sb.AppendLine("<div class='report-title'>ANNUAL CATCHMENT POPULATION SUMMARY REPORT</div>");
            sb.AppendLine($"<div class='report-subtitle'>BRGY. PUTINGKAHOY</div>");
            sb.AppendLine($"<div class='report-subtitle'>YEAR: {year}</div>");
            sb.AppendLine("</div>");

            // Table
            sb.AppendLine("<table class='report-table'>");
            sb.AppendLine("<thead>");
            sb.AppendLine("<tr>");
            sb.AppendLine("<th style='width: 200px;'>LESS GROUP</th>");
            sb.AppendLine("<th>MALE</th>");
            sb.AppendLine("<th>FEMALE</th>");
            sb.AppendLine("<th>TOTAL</th>");
            sb.AppendLine("</tr>");
            sb.AppendLine("</thead>");
            sb.AppendLine("<tbody>");

            foreach (var group in catchmentData)
            {
                var isTotal = group.AgeGroup == "TOTAL";
                var rowClass = isTotal ? "total-row" : "";
                sb.AppendLine($"<tr class='{rowClass}'>");
                sb.AppendLine($"<td class='age-group'>{group.AgeGroup}</td>");
                sb.AppendLine($"<td>{group.Male}</td>");
                sb.AppendLine($"<td>{group.Female}</td>");
                sb.AppendLine($"<td>{group.Total}</td>");
                sb.AppendLine("</tr>");
            }

            sb.AppendLine("</tbody>");
            sb.AppendLine("</table>");

            // Footer
            var currentDate = DateTime.Now.ToString("MM-dd-yy");
            sb.AppendLine("<div class='report-footer'>");
            sb.AppendLine("<div class='date-section'>");
            sb.AppendLine($"<div class='date-value'>{currentDate}</div>");
            sb.AppendLine("<div>DATE SUBMITTED;</div>");
            sb.AppendLine("</div>");
            sb.AppendLine("<div class='submitted-section'>");
            sb.AppendLine("<div>SUBMITTED BY;</div>");
            if (!string.IsNullOrEmpty(submittedBy))
            {
                sb.AppendLine($"<div style='margin-top: 30px;'><strong>{submittedBy.ToUpper()}</strong></div>");
            }
            else if (bhwProfile != null)
            {
                sb.AppendLine($"<div style='margin-top: 30px;'><strong>{bhwProfile.FirstName.ToUpper()} {bhwProfile.MiddleName?.ToUpper()} {bhwProfile.LastName.ToUpper()}</strong></div>");
            }
            else
            {
                sb.AppendLine("<div style='margin-top: 30px;'>_____________</div>");
            }
            sb.AppendLine("</div>");
            sb.AppendLine("</div>");

            // Auto-print script
            sb.AppendLine("<script>");
            sb.AppendLine("(function() {");
            sb.AppendLine("  function triggerPrint() {");
            sb.AppendLine("    window.print();");
            sb.AppendLine("  }");
            sb.AppendLine("  if (document.readyState === 'complete') {");
            sb.AppendLine("    setTimeout(triggerPrint, 500);");
            sb.AppendLine("  } else {");
            sb.AppendLine("    window.addEventListener('load', function() {");
            sb.AppendLine("      setTimeout(triggerPrint, 500);");
            sb.AppendLine("    });");
            sb.AppendLine("  }");
            sb.AppendLine("})();");
            sb.AppendLine("</script>");

            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            return sb.ToString();
        }

        private string GeneratePopAgeIndividualHtml(Dictionary<int, IndividualAgeData> individualAgeData, BHWProfile? bhwProfile, int year, string? submittedBy)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html>");
            sb.AppendLine("<head>");
            sb.AppendLine("<meta charset='UTF-8'>");
            sb.AppendLine("<title>POP AGE INDIVIDUAL</title>");
            sb.AppendLine("<style>");
            sb.AppendLine("@page { margin: 0.3in; size: legal landscape; }");
            sb.AppendLine("@media print { @page { size: 14in 8.5in; margin: 0.3in; } body { width: 100% !important; max-width: 100% !important; margin: 0 !important; padding: 0.3in !important; box-shadow: none !important; } }");
            sb.AppendLine("* { margin: 0; padding: 0; box-sizing: border-box; }");
            sb.AppendLine("body { font-family: 'Times New Roman', serif; font-size: 9pt; margin: 10px auto; padding: 10px; background: white; }");
            sb.AppendLine(".report-header { text-align: center; margin-bottom: 10px; }");
            sb.AppendLine(".report-title { font-size: 12pt; font-weight: bold; margin-bottom: 5px; }");
            sb.AppendLine(".report-table { width: 100%; border-collapse: collapse; margin: 10px 0; font-size: 8pt; }");
            sb.AppendLine(".report-table th, .report-table td { border: 1px solid #000; padding: 3px; text-align: center; }");
            sb.AppendLine(".report-table th { background-color: #f0f0f0; font-weight: bold; }");
            sb.AppendLine(".report-table .age-label { text-align: left; padding-left: 5px; }");
            sb.AppendLine(".report-table .total-row { font-weight: bold; background-color: #e0e0e0; }");
            sb.AppendLine(".age-block { display: inline-block; vertical-align: top; width: 32%; margin: 0 0.5%; }");
            sb.AppendLine(".grand-total { margin-top: 15px; text-align: center; font-weight: bold; font-size: 10pt; }");
            sb.AppendLine("</style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");

            // Header
            sb.AppendLine("<div class='report-header'>");
            sb.AppendLine("<div class='report-title'>POP AGE INDIVIDUAL</div>");
            sb.AppendLine($"<div>YEAR: {year}</div>");
            sb.AppendLine("</div>");

            // Organize data into three blocks: 0-29, 30-59, 60-88+
            var block1 = new List<IndividualAgeData>(); // 0-29
            var block2 = new List<IndividualAgeData>(); // 30-59
            var block3 = new List<IndividualAgeData>(); // 60-88+

            // Add infant categories first
            if (individualAgeData.ContainsKey(-1))
                block1.Add(individualAgeData[-1]);
            if (individualAgeData.ContainsKey(-2))
                block1.Add(individualAgeData[-2]);

            // Add ages 1-29
            for (int age = 1; age <= 29; age++)
            {
                if (individualAgeData.ContainsKey(age))
                    block1.Add(individualAgeData[age]);
                else
                    block1.Add(new IndividualAgeData { AgeLabel = age.ToString(), Male = 0, Female = 0, Total = 0 });
            }

            // Add ages 30-59
            for (int age = 30; age <= 59; age++)
            {
                if (individualAgeData.ContainsKey(age))
                    block2.Add(individualAgeData[age]);
                else
                    block2.Add(new IndividualAgeData { AgeLabel = age.ToString(), Male = 0, Female = 0, Total = 0 });
            }

            // Add ages 60-88+
            for (int age = 60; age <= 88; age++)
            {
                if (individualAgeData.ContainsKey(age))
                    block3.Add(individualAgeData[age]);
                else
                    block3.Add(new IndividualAgeData { AgeLabel = age.ToString(), Male = 0, Female = 0, Total = 0 });
            }

            // Add "ABOVE" category for ages > 88
            var above88 = individualAgeData.Where(kvp => kvp.Key > 88).ToList();
            if (above88.Any())
            {
                var aboveData = new IndividualAgeData { AgeLabel = "ABOVE", Male = 0, Female = 0, Total = 0 };
                foreach (var item in above88)
                {
                    aboveData.Male += item.Value.Male;
                    aboveData.Female += item.Value.Female;
                    aboveData.Total += item.Value.Total;
                }
                block3.Add(aboveData);
            }

            // Calculate totals for each block
            var block1Total = new IndividualAgeData 
            { 
                AgeLabel = "TOTAL", 
                Male = block1.Sum(b => b.Male), 
                Female = block1.Sum(b => b.Female), 
                Total = block1.Sum(b => b.Total) 
            };
            var block2Total = new IndividualAgeData 
            { 
                AgeLabel = "TOTAL", 
                Male = block2.Sum(b => b.Male), 
                Female = block2.Sum(b => b.Female), 
                Total = block2.Sum(b => b.Total) 
            };
            var block3Total = new IndividualAgeData 
            { 
                AgeLabel = "TOTAL", 
                Male = block3.Sum(b => b.Male), 
                Female = block3.Sum(b => b.Female), 
                Total = block3.Sum(b => b.Total) 
            };

            // Render three blocks side by side
            sb.AppendLine("<div style='display: flex; gap: 5px;'>");

            // Block 1: 0-29
            sb.AppendLine("<div class='age-block'>");
            sb.AppendLine("<table class='report-table'>");
            sb.AppendLine("<thead><tr><th>AGE</th><th>MALE</th><th>FEMALE</th><th>TOTAL</th></tr></thead>");
            sb.AppendLine("<tbody>");
            foreach (var item in block1)
            {
                sb.AppendLine("<tr>");
                sb.AppendLine($"<td class='age-label'>{item.AgeLabel}</td>");
                sb.AppendLine($"<td>{item.Male}</td>");
                sb.AppendLine($"<td>{item.Female}</td>");
                sb.AppendLine($"<td>{item.Total}</td>");
                sb.AppendLine("</tr>");
            }
            sb.AppendLine($"<tr class='total-row'><td class='age-label'>{block1Total.AgeLabel}</td><td>{block1Total.Male}</td><td>{block1Total.Female}</td><td>{block1Total.Total}</td></tr>");
            sb.AppendLine("</tbody></table></div>");

            // Block 2: 30-59
            sb.AppendLine("<div class='age-block'>");
            sb.AppendLine("<table class='report-table'>");
            sb.AppendLine("<thead><tr><th>AGE</th><th>MALE</th><th>FEMALE</th><th>TOTAL</th></tr></thead>");
            sb.AppendLine("<tbody>");
            foreach (var item in block2)
            {
                sb.AppendLine("<tr>");
                sb.AppendLine($"<td class='age-label'>{item.AgeLabel}</td>");
                sb.AppendLine($"<td>{item.Male}</td>");
                sb.AppendLine($"<td>{item.Female}</td>");
                sb.AppendLine($"<td>{item.Total}</td>");
                sb.AppendLine("</tr>");
            }
            sb.AppendLine($"<tr class='total-row'><td class='age-label'>{block2Total.AgeLabel}</td><td>{block2Total.Male}</td><td>{block2Total.Female}</td><td>{block2Total.Total}</td></tr>");
            sb.AppendLine("</tbody></table></div>");

            // Block 3: 60-88+
            sb.AppendLine("<div class='age-block'>");
            sb.AppendLine("<table class='report-table'>");
            sb.AppendLine("<thead><tr><th>AGE</th><th>MALE</th><th>FEMALE</th><th>TOTAL</th></tr></thead>");
            sb.AppendLine("<tbody>");
            foreach (var item in block3)
            {
                sb.AppendLine("<tr>");
                sb.AppendLine($"<td class='age-label'>{item.AgeLabel}</td>");
                sb.AppendLine($"<td>{item.Male}</td>");
                sb.AppendLine($"<td>{item.Female}</td>");
                sb.AppendLine($"<td>{item.Total}</td>");
                sb.AppendLine("</tr>");
            }
            sb.AppendLine($"<tr class='total-row'><td class='age-label'>{block3Total.AgeLabel}</td><td>{block3Total.Male}</td><td>{block3Total.Female}</td><td>{block3Total.Total}</td></tr>");
            sb.AppendLine("</tbody></table></div>");

            sb.AppendLine("</div>");

            // Grand Total
            var grandTotalMale = block1Total.Male + block2Total.Male + block3Total.Male;
            var grandTotalFemale = block1Total.Female + block2Total.Female + block3Total.Female;
            var grandTotal = block1Total.Total + block2Total.Total + block3Total.Total;

            sb.AppendLine("<div class='grand-total'>");
            sb.AppendLine($"<div>MALE: {grandTotalMale}</div>");
            sb.AppendLine($"<div>FEMALE: {grandTotalFemale}</div>");
            sb.AppendLine($"<div>total: {grandTotal}</div>");
            sb.AppendLine("</div>");

            // Footer
            var currentDate = DateTime.Now.ToString("MM-dd-yy");
            sb.AppendLine("<div style='margin-top: 20px; display: flex; justify-content: space-between; font-size: 9pt;'>");
            sb.AppendLine("<div>");
            sb.AppendLine($"<div style='text-decoration: underline;'>{currentDate}</div>");
            sb.AppendLine("<div>Date</div>");
            sb.AppendLine("</div>");
            sb.AppendLine("<div style='text-align: right;'>");
            sb.AppendLine("<div>Submitted By:</div>");
            if (!string.IsNullOrEmpty(submittedBy))
            {
                sb.AppendLine($"<div style='margin-top: 20px;'><strong>{submittedBy.ToUpper()}</strong></div>");
            }
            else if (bhwProfile != null)
            {
                sb.AppendLine($"<div style='margin-top: 20px;'><strong>{bhwProfile.FirstName.ToUpper()} {bhwProfile.MiddleName?.ToUpper()} {bhwProfile.LastName.ToUpper()}</strong></div>");
            }
            else
            {
                sb.AppendLine("<div style='margin-top: 20px;'>_____________</div>");
            }
            sb.AppendLine("</div>");
            sb.AppendLine("</div>");

            // Auto-print script
            sb.AppendLine("<script>");
            sb.AppendLine("(function() {");
            sb.AppendLine("  function triggerPrint() {");
            sb.AppendLine("    window.print();");
            sb.AppendLine("  }");
            sb.AppendLine("  if (document.readyState === 'complete') {");
            sb.AppendLine("    setTimeout(triggerPrint, 500);");
            sb.AppendLine("  } else {");
            sb.AppendLine("    window.addEventListener('load', function() {");
            sb.AppendLine("      setTimeout(triggerPrint, 500);");
            sb.AppendLine("    });");
            sb.AppendLine("  }");
            sb.AppendLine("})();");
            sb.AppendLine("</script>");

            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            return sb.ToString();
        }

        private class WRADistributionData
        {
            public int Age10To14 { get; set; }
            public int Age15To19 { get; set; }
            public int Age20To49 { get; set; }
            public int TotalWRA15To49 { get; set; }
            public int TotalWRA { get; set; }
        }

        private class CatchmentAgeGroupData
        {
            public string AgeGroup { get; set; } = string.Empty;
            public int Male { get; set; }
            public int Female { get; set; }
            public int Total { get; set; }
        }

        private class IndividualAgeData
        {
            public string AgeLabel { get; set; } = string.Empty;
            public int Male { get; set; }
            public int Female { get; set; }
            public int Total { get; set; }
        }
    }

    [ApiController]
    [Route("api/bhw-assignments")]
    [Authorize]
    public class BHWAssignmentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public BHWAssignmentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int? bhwProfileId, [FromQuery] string? status)
        {
            var query = _context.BHWAssignments.Include(a => a.BHWProfile).AsQueryable();
            
            if (bhwProfileId.HasValue)
            {
                query = query.Where(a => a.BHWProfileId == bhwProfileId.Value);
            }
            
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(a => a.Status == status);
            }
            
            var assignments = await query.OrderByDescending(a => a.AssignmentDate).ToListAsync();
            return Ok(assignments);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var assignment = await _context.BHWAssignments
                .Include(a => a.BHWProfile)
                .FirstOrDefaultAsync(a => a.Id == id);
            
            if (assignment == null) return NotFound();
            return Ok(assignment);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateBHWAssignmentDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var bhwProfile = await _context.BHWProfiles.FindAsync(dto.BHWProfileId);
                if (bhwProfile == null)
                {
                    return BadRequest(new { message = "BHW Profile not found" });
                }

                var assignment = new BHWAssignment
                {
                    BHWProfileId = dto.BHWProfileId,
                    ZoneName = dto.ZoneName,
                    ZoneDescription = dto.ZoneDescription,
                    CoverageArea = dto.CoverageArea,
                    AssignmentDate = dto.AssignmentDate,
                    EndDate = dto.EndDate,
                    Status = dto.Status,
                    Notes = dto.Notes,
                    AssignedBy = dto.AssignedBy,
                    CreatedAt = DateTime.UtcNow
                };

                _context.BHWAssignments.Add(assignment);
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(GetById), new { id = assignment.Id }, assignment);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the assignment", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateBHWAssignmentDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var assignment = await _context.BHWAssignments.FindAsync(id);
                if (assignment == null) return NotFound();

                assignment.ZoneName = dto.ZoneName;
                assignment.ZoneDescription = dto.ZoneDescription;
                assignment.CoverageArea = dto.CoverageArea;
                assignment.AssignmentDate = dto.AssignmentDate;
                assignment.EndDate = dto.EndDate;
                assignment.Status = dto.Status;
                assignment.Notes = dto.Notes;
                assignment.AssignedBy = dto.AssignedBy;

                await _context.SaveChangesAsync();
                return Ok(assignment);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the assignment", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var assignment = await _context.BHWAssignments.FindAsync(id);
            if (assignment == null) return NotFound();

            _context.BHWAssignments.Remove(assignment);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }

    [ApiController]
    [Route("api/bhw-visit-logs")]
    [Authorize]
    public class BHWVisitLogsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public BHWVisitLogsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int? bhwProfileId, [FromQuery] int? residentId, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            var query = _context.BHWVisitLogs
                .Include(v => v.BHWProfile)
                .Include(v => v.Resident)
                .AsQueryable();
            
            if (bhwProfileId.HasValue)
            {
                query = query.Where(v => v.BHWProfileId == bhwProfileId.Value);
            }
            
            if (residentId.HasValue)
            {
                query = query.Where(v => v.ResidentId == residentId.Value);
            }
            
            if (fromDate.HasValue)
            {
                query = query.Where(v => v.VisitDate >= fromDate.Value);
            }
            
            if (toDate.HasValue)
            {
                query = query.Where(v => v.VisitDate <= toDate.Value);
            }
            
            var logs = await query.OrderByDescending(v => v.VisitDate).ToListAsync();
            return Ok(logs);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var log = await _context.BHWVisitLogs
                .Include(v => v.BHWProfile)
                .Include(v => v.Resident)
                .FirstOrDefaultAsync(v => v.Id == id);
            
            if (log == null) return NotFound();
            return Ok(log);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateBHWVisitLogDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var bhwProfile = await _context.BHWProfiles.FindAsync(dto.BHWProfileId);
                if (bhwProfile == null)
                {
                    return BadRequest(new { message = "BHW Profile not found" });
                }

                if (dto.ResidentId.HasValue)
                {
                    var resident = await _context.Residents.FindAsync(dto.ResidentId.Value);
                    if (resident == null)
                    {
                        return BadRequest(new { message = "Resident not found" });
                    }
                }

                var log = new BHWVisitLog
                {
                    BHWProfileId = dto.BHWProfileId,
                    ResidentId = dto.ResidentId,
                    VisitedPersonName = dto.VisitedPersonName,
                    Address = dto.Address,
                    VisitDate = dto.VisitDate,
                    VisitType = dto.VisitType,
                    VisitPurpose = dto.VisitPurpose,
                    Findings = dto.Findings,
                    ActionsTaken = dto.ActionsTaken,
                    Recommendations = dto.Recommendations,
                    ReferralStatus = dto.ReferralStatus,
                    ReferralNotes = dto.ReferralNotes,
                    Notes = dto.Notes,
                    CreatedAt = DateTime.UtcNow
                };

                _context.BHWVisitLogs.Add(log);
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(GetById), new { id = log.Id }, log);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the visit log", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateBHWVisitLogDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var log = await _context.BHWVisitLogs.FindAsync(id);
                if (log == null) return NotFound();

                log.ResidentId = dto.ResidentId;
                log.VisitedPersonName = dto.VisitedPersonName;
                log.Address = dto.Address;
                log.VisitDate = dto.VisitDate;
                log.VisitType = dto.VisitType;
                log.VisitPurpose = dto.VisitPurpose;
                log.Findings = dto.Findings;
                log.ActionsTaken = dto.ActionsTaken;
                log.Recommendations = dto.Recommendations;
                log.ReferralStatus = dto.ReferralStatus;
                log.ReferralNotes = dto.ReferralNotes;
                log.Notes = dto.Notes;

                await _context.SaveChangesAsync();
                return Ok(log);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the visit log", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var log = await _context.BHWVisitLogs.FindAsync(id);
            if (log == null) return NotFound();

            _context.BHWVisitLogs.Remove(log);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }

    [ApiController]
    [Route("api/bhw-trainings")]
    [Authorize]
    public class BHWTrainingsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public BHWTrainingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int? bhwProfileId, [FromQuery] string? status)
        {
            var query = _context.BHWTrainings.Include(t => t.BHWProfile).AsQueryable();
            
            if (bhwProfileId.HasValue)
            {
                query = query.Where(t => t.BHWProfileId == bhwProfileId.Value);
            }
            
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(t => t.Status == status);
            }
            
            var trainings = await query.OrderByDescending(t => t.TrainingDate).ToListAsync();
            return Ok(trainings);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var training = await _context.BHWTrainings
                .Include(t => t.BHWProfile)
                .FirstOrDefaultAsync(t => t.Id == id);
            
            if (training == null) return NotFound();
            return Ok(training);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateBHWTrainingDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var bhwProfile = await _context.BHWProfiles.FindAsync(dto.BHWProfileId);
                if (bhwProfile == null)
                {
                    return BadRequest(new { message = "BHW Profile not found" });
                }

                var training = new BHWTraining
                {
                    BHWProfileId = dto.BHWProfileId,
                    TrainingTitle = dto.TrainingTitle,
                    Description = dto.Description,
                    TrainingProvider = dto.TrainingProvider,
                    TrainingDate = dto.TrainingDate,
                    TrainingEndDate = dto.TrainingEndDate,
                    TrainingType = dto.TrainingType,
                    Status = dto.Status,
                    CertificateNumber = dto.CertificateNumber,
                    CertificatePath = dto.CertificatePath,
                    Notes = dto.Notes,
                    CreatedAt = DateTime.UtcNow
                };

                _context.BHWTrainings.Add(training);
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(GetById), new { id = training.Id }, training);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the training", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateBHWTrainingDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var training = await _context.BHWTrainings.FindAsync(id);
                if (training == null) return NotFound();

                training.TrainingTitle = dto.TrainingTitle;
                training.Description = dto.Description;
                training.TrainingProvider = dto.TrainingProvider;
                training.TrainingDate = dto.TrainingDate;
                training.TrainingEndDate = dto.TrainingEndDate;
                training.TrainingType = dto.TrainingType;
                training.Status = dto.Status;
                training.CertificateNumber = dto.CertificateNumber;
                training.CertificatePath = dto.CertificatePath;
                training.Notes = dto.Notes;

                await _context.SaveChangesAsync();
                return Ok(training);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the training", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var training = await _context.BHWTrainings.FindAsync(id);
            if (training == null) return NotFound();

            _context.BHWTrainings.Remove(training);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }

    [ApiController]
    [Route("api/bhw-incentives")]
    [Authorize]
    public class BHWIncentivesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public BHWIncentivesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int? bhwProfileId, [FromQuery] string? paymentStatus)
        {
            var query = _context.BHWIncentives.Include(i => i.BHWProfile).AsQueryable();
            
            if (bhwProfileId.HasValue)
            {
                query = query.Where(i => i.BHWProfileId == bhwProfileId.Value);
            }
            
            if (!string.IsNullOrEmpty(paymentStatus))
            {
                query = query.Where(i => i.PaymentStatus == paymentStatus);
            }
            
            var incentives = await query.OrderByDescending(i => i.IncentiveDate).ToListAsync();
            return Ok(incentives);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var incentive = await _context.BHWIncentives
                .Include(i => i.BHWProfile)
                .FirstOrDefaultAsync(i => i.Id == id);
            
            if (incentive == null) return NotFound();
            return Ok(incentive);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateBHWIncentiveDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var bhwProfile = await _context.BHWProfiles.FindAsync(dto.BHWProfileId);
                if (bhwProfile == null)
                {
                    return BadRequest(new { message = "BHW Profile not found" });
                }

                var incentive = new BHWIncentive
                {
                    BHWProfileId = dto.BHWProfileId,
                    IncentiveType = dto.IncentiveType,
                    Amount = dto.Amount,
                    IncentiveDate = dto.IncentiveDate,
                    PaymentStatus = dto.PaymentStatus,
                    PaymentDate = dto.PaymentDate,
                    PaymentMethod = dto.PaymentMethod,
                    ReferenceNumber = dto.ReferenceNumber,
                    Remarks = dto.Remarks,
                    ProcessedBy = dto.ProcessedBy,
                    CreatedAt = DateTime.UtcNow
                };

                _context.BHWIncentives.Add(incentive);
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(GetById), new { id = incentive.Id }, incentive);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the incentive", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateBHWIncentiveDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var incentive = await _context.BHWIncentives.FindAsync(id);
                if (incentive == null) return NotFound();

                incentive.IncentiveType = dto.IncentiveType;
                incentive.Amount = dto.Amount;
                incentive.IncentiveDate = dto.IncentiveDate;
                incentive.PaymentStatus = dto.PaymentStatus;
                incentive.PaymentDate = dto.PaymentDate;
                incentive.PaymentMethod = dto.PaymentMethod;
                incentive.ReferenceNumber = dto.ReferenceNumber;
                incentive.Remarks = dto.Remarks;
                incentive.ProcessedBy = dto.ProcessedBy;

                await _context.SaveChangesAsync();
                return Ok(incentive);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the incentive", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var incentive = await _context.BHWIncentives.FindAsync(id);
            if (incentive == null) return NotFound();

            _context.BHWIncentives.Remove(incentive);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}


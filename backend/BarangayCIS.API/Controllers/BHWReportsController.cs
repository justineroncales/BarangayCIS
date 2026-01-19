using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BarangayCIS.API.Data;
using BarangayCIS.API.Models;
using ClosedXML.Excel;

namespace BarangayCIS.API.Controllers
{
    [ApiController]
    [Route("api/bhw-reports")]
    [Authorize]
    public class BHWReportsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public BHWReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ========== DELIVERIES ENDPOINTS ==========
        
        [HttpGet("deliveries")]
        public async Task<IActionResult> GetDeliveries([FromQuery] int? bhwProfileId, [FromQuery] int? year)
        {
            var query = _context.Deliveries.Include(d => d.BHWProfile).AsQueryable();
            
            if (bhwProfileId.HasValue)
            {
                query = query.Where(d => d.BHWProfileId == bhwProfileId.Value);
            }
            
            if (year.HasValue)
            {
                query = query.Where(d => d.Year == year.Value);
            }
            
            var deliveries = await query.OrderByDescending(d => d.DateOfBirth).ToListAsync();
            return Ok(deliveries);
        }

        [HttpGet("deliveries/{id}")]
        public async Task<IActionResult> GetDelivery(int id)
        {
            var delivery = await _context.Deliveries
                .Include(d => d.BHWProfile)
                .FirstOrDefaultAsync(d => d.Id == id);
            
            if (delivery == null) return NotFound();
            return Ok(delivery);
        }

        [HttpPost("deliveries")]
        public async Task<IActionResult> CreateDelivery([FromBody] Delivery delivery)
        {
            // Log received data for debugging
            Console.WriteLine($"Received Delivery - BHWProfileId: {delivery?.BHWProfileId}, MotherName: {delivery?.MotherName}");
            
            if (!ModelState.IsValid)
            {
                // Log validation errors
                foreach (var error in ModelState)
                {
                    Console.WriteLine($"Validation Error - {error.Key}: {string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
                }
                // Return validation errors in ASP.NET Core format
                return BadRequest(ModelState);
            }

            try
            {
                // Validate required fields
                if (delivery.BHWProfileId <= 0)
                {
                    return BadRequest(new { message = "BHW Profile ID is required and must be greater than 0" });
                }
                
                if (string.IsNullOrWhiteSpace(delivery.MotherName))
                {
                    return BadRequest(new { message = "Mother Name is required" });
                }
                
                if (string.IsNullOrWhiteSpace(delivery.ChildName))
                {
                    return BadRequest(new { message = "Child Name is required" });
                }
                
                if (string.IsNullOrWhiteSpace(delivery.Gender))
                {
                    return BadRequest(new { message = "Gender is required" });
                }
                
                if (delivery.DateOfBirth == default(DateTime))
                {
                    return BadRequest(new { message = "Date of Birth is required" });
                }

                // Verify BHW Profile exists
                var bhwExists = await _context.BHWProfiles.AnyAsync(b => b.Id == delivery.BHWProfileId);
                if (!bhwExists)
                {
                    return BadRequest(new { message = $"BHW Profile with ID {delivery.BHWProfileId} does not exist" });
                }

                delivery.Year = delivery.DateOfBirth.Year;
                delivery.CreatedAt = DateTime.UtcNow;
                
                _context.Deliveries.Add(delivery);
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(GetDelivery), new { id = delivery.Id }, delivery);
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
            {
                var innerException = dbEx.InnerException?.Message ?? dbEx.Message;
                var fullError = dbEx.InnerException?.ToString() ?? dbEx.ToString();
                Console.WriteLine($"Database Error: {innerException}");
                Console.WriteLine($"Full Exception: {fullError}");
                
                // Check for table not found error
                if (innerException.Contains("Invalid object name") || innerException.Contains("does not exist") || innerException.Contains("Deliveries"))
                {
                    return StatusCode(500, new { 
                        message = "Database table 'Deliveries' does not exist. Please restart the API server to create the table.", 
                        error = innerException
                    });
                }
                
                // Check for foreign key constraint violation
                if (innerException.Contains("FOREIGN KEY") || innerException.Contains("The INSERT statement conflicted"))
                {
                    return BadRequest(new { 
                        message = "Invalid BHW Profile ID. The selected BHW Profile does not exist.", 
                        error = innerException
                    });
                }
                
                return StatusCode(500, new { 
                    message = "Database error while creating delivery record", 
                    error = innerException,
                    details = fullError
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General error: {ex.Message}");
                Console.WriteLine($"Full exception: {ex}");
                return StatusCode(500, new { 
                    message = "An error occurred while creating the delivery record", 
                    error = ex.Message,
                    details = ex.ToString()
                });
            }
        }

        [HttpPut("deliveries/{id}")]
        public async Task<IActionResult> UpdateDelivery(int id, [FromBody] Delivery delivery)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var existing = await _context.Deliveries.FindAsync(id);
                if (existing == null) return NotFound();

                existing.BHWProfileId = delivery.BHWProfileId;
                existing.MotherName = delivery.MotherName;
                existing.ChildName = delivery.ChildName;
                existing.PurokSitio = delivery.PurokSitio;
                existing.Gender = delivery.Gender;
                existing.DateOfBirth = delivery.DateOfBirth;
                existing.Year = delivery.DateOfBirth.Year;
                existing.TimeOfBirth = delivery.TimeOfBirth;
                existing.Weight = delivery.Weight;
                existing.Height = delivery.Height;
                existing.PlaceOfBirth = delivery.PlaceOfBirth;
                existing.DeliveryType = delivery.DeliveryType;
                existing.BCGAndHepaB = delivery.BCGAndHepaB;
                existing.AttendedBy = delivery.AttendedBy;
                existing.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return Ok(existing);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the delivery record", error = ex.Message });
            }
        }

        [HttpDelete("deliveries/{id}")]
        public async Task<IActionResult> DeleteDelivery(int id)
        {
            var delivery = await _context.Deliveries.FindAsync(id);
            if (delivery == null) return NotFound();

            _context.Deliveries.Remove(delivery);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("deliveries/template/download")]
        [AllowAnonymous]
        public IActionResult DownloadDeliveryTemplate()
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Deliveries");

            // Instructions
            worksheet.Cell(1, 1).Value = "DELIVERIES LOGBOOK TEMPLATE - INSTRUCTIONS";
            worksheet.Cell(1, 1).Style.Font.Bold = true;
            worksheet.Cell(1, 1).Style.Font.FontSize = 14;
            worksheet.Cell(2, 1).Value = "Delete example rows (rows 7-8) before entering your data.";
            worksheet.Cell(3, 1).Value = "For Date: Use MM/DD/YYYY format (e.g., 01/15/2025). For Time: Use format like '2:13 AM' or '7:20 PM'.";
            worksheet.Cell(4, 1).Value = "For Gender: Enter 'M' for Male or 'F' for Female. For Delivery Type: Enter 'CS' for Cesarean or 'NSD' for Normal.";
            worksheet.Cell(5, 1).Value = "";

            // Headers
            var headers = new[]
            {
                "No", "Mother Name*", "Child Name*", "Purok/Sitio", "Gender* (M/F)",
                "Date* (MM/DD/YYYY)", "Time (HH:MM AM/PM)", "Weight (kg)", "Height (cm)",
                "Place of Birth", "Delivery Type (CS/NSD)", "BCG & HEPA B", "Attended By"
            };

            for (int i = 0; i < headers.Length; i++)
            {
                var cell = worksheet.Cell(6, i + 1);
                cell.Value = headers[i];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.PatternType = XLFillPatternValues.Solid;
                cell.Style.Fill.BackgroundColor = XLColor.LightBlue;
                cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            }

            // Example rows
            var exampleRow1 = new object[]
            {
                1, "ARLENE REMOLIN", "ARIANNE PIZZARA JUNIO", "SAN JUAN ST. BLK 4 D-5", "F",
                "07/02/2025", "2:13 AM", "2.84 kg", "51 CM",
                "BALAYAN BAYVIEW", "CS", "HEPA B 7-2-2025 BCG-7-2-28", "ORB SAUNAR AGE: 19 YRS DL G2 P1"
            };

            var exampleRow2 = new object[]
            {
                2, "JUVILYN NANIT PAGARA", "ANALAH BRIELLE PAGARA SAGUBAN", "M. APACIBLESH BIK3 B5", "F",
                "08/25/2025", "7:20 PM", "2.5 KG", "49 cm",
                "TUY LYING IN P.", "NSD", "HEPA B 8-25-25 BCG 8-25-24", "MARY ANN COTOE"
            };

            for (int i = 0; i < exampleRow1.Length; i++)
            {
                worksheet.Cell(7, i + 1).Value = exampleRow1[i]?.ToString() ?? "";
                worksheet.Cell(8, i + 1).Value = exampleRow2[i]?.ToString() ?? "";
            }

            // Column widths
            worksheet.Column(2).Width = 25; // Mother Name
            worksheet.Column(3).Width = 25; // Child Name
            worksheet.Column(4).Width = 25; // Purok/Sitio
            worksheet.Column(6).Width = 15; // Date
            worksheet.Column(7).Width = 15; // Time
            worksheet.Column(9).Width = 12; // Height
            worksheet.Column(10).Width = 20; // Place of Birth
            worksheet.Column(12).Width = 30; // BCG & HEPA B
            worksheet.Column(13).Width = 30; // Attended By
            worksheet.Columns().AdjustToContents();

            worksheet.SheetView.FreezeRows(6);

            var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            var fileName = $"Delivery_Template_{DateTime.Now:yyyyMMdd}.xlsx";
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        // ========== KRA REPORTS ENDPOINTS ==========
        
        [HttpGet("kra")]
        public async Task<IActionResult> GetKRAReports([FromQuery] int? bhwProfileId, [FromQuery] int? year, [FromQuery] int? month)
        {
            var query = _context.KRAReports.Include(k => k.BHWProfile).AsQueryable();
            
            if (bhwProfileId.HasValue)
            {
                query = query.Where(k => k.BHWProfileId == bhwProfileId.Value);
            }
            
            if (year.HasValue)
            {
                query = query.Where(k => k.Year == year.Value);
            }
            
            if (month.HasValue)
            {
                query = query.Where(k => k.Month == month.Value);
            }
            
            var reports = await query.OrderByDescending(k => k.Year).ThenByDescending(k => k.Month).ToListAsync();
            return Ok(reports);
        }

        [HttpGet("kra/{id}")]
        public async Task<IActionResult> GetKRAReport(int id)
        {
            var report = await _context.KRAReports
                .Include(k => k.BHWProfile)
                .FirstOrDefaultAsync(k => k.Id == id);
            
            if (report == null) return NotFound();
            return Ok(report);
        }

        [HttpPost("kra")]
        public async Task<IActionResult> CreateKRAReport([FromBody] KRAReport report)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                report.CreatedAt = DateTime.UtcNow;
                
                _context.KRAReports.Add(report);
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(GetKRAReport), new { id = report.Id }, report);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the KRA report", error = ex.Message });
            }
        }

        [HttpPut("kra/{id}")]
        public async Task<IActionResult> UpdateKRAReport(int id, [FromBody] KRAReport report)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var existing = await _context.KRAReports.FindAsync(id);
                if (existing == null) return NotFound();

                existing.BHWProfileId = report.BHWProfileId;
                existing.Year = report.Year;
                existing.Month = report.Month;
                existing.PillsPOP_10To14 = report.PillsPOP_10To14;
                existing.PillsPOP_15To19 = report.PillsPOP_15To19;
                existing.PillsPOP_20Plus = report.PillsPOP_20Plus;
                existing.PillsCOC_10To14 = report.PillsCOC_10To14;
                existing.PillsCOC_15To19 = report.PillsCOC_15To19;
                existing.PillsCOC_20Plus = report.PillsCOC_20Plus;
                existing.DMPA_10To14 = report.DMPA_10To14;
                existing.DMPA_15To19 = report.DMPA_15To19;
                existing.DMPA_20Plus = report.DMPA_20Plus;
                existing.Condom_10To14 = report.Condom_10To14;
                existing.Condom_15To19 = report.Condom_15To19;
                existing.Condom_20Plus = report.Condom_20Plus;
                existing.Implant_10To14 = report.Implant_10To14;
                existing.Implant_15To19 = report.Implant_15To19;
                existing.Implant_20Plus = report.Implant_20Plus;
                existing.BTL_10To14 = report.BTL_10To14;
                existing.BTL_15To19 = report.BTL_15To19;
                existing.BTL_20Plus = report.BTL_20Plus;
                existing.LAM_10To14 = report.LAM_10To14;
                existing.LAM_15To19 = report.LAM_15To19;
                existing.LAM_20Plus = report.LAM_20Plus;
                existing.IUD_10To14 = report.IUD_10To14;
                existing.IUD_15To19 = report.IUD_15To19;
                existing.IUD_20Plus = report.IUD_20Plus;
                existing.Deliveries_10To14 = report.Deliveries_10To14;
                existing.Deliveries_15To19 = report.Deliveries_15To19;
                existing.Deliveries_20Plus = report.Deliveries_20Plus;
                existing.TeenagePregnancies = report.TeenagePregnancies;
                existing.Notes = report.Notes;
                existing.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return Ok(existing);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the KRA report", error = ex.Message });
            }
        }

        [HttpDelete("kra/{id}")]
        public async Task<IActionResult> DeleteKRAReport(int id)
        {
            var report = await _context.KRAReports.FindAsync(id);
            if (report == null) return NotFound();

            _context.KRAReports.Remove(report);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("kra/template/download")]
        [AllowAnonymous]
        public IActionResult DownloadKRATemplate()
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("KRA Report");

            // Instructions
            worksheet.Cell(1, 1).Value = "KEY RESULTS AREA (KRA) REPORT TEMPLATE - INSTRUCTIONS";
            worksheet.Cell(1, 1).Style.Font.Bold = true;
            worksheet.Cell(1, 1).Style.Font.FontSize = 14;
            worksheet.Cell(2, 1).Value = "Enter numeric values only. Each row represents one month. Delete example rows before entering your data.";
            worksheet.Cell(3, 1).Value = "Age groups: 10-14 y.o., 15-19 y.o., 20+ y.o. Teenage Pregnancies is total count (no age breakdown).";
            worksheet.Cell(4, 1).Value = "";

            // Headers - Complex structure matching the report format
            var monthHeaders = new[] { "Month", "Year" };
            var methodHeaders = new[] { "PILLS-POP", "PILLS-COC", "DMPA", "CONDOM", "IMPLANT", "BTL", "LAM", "IUD", "DELIVERIES", "TEENAGE PREGNANCIES" };
            var ageGroups = new[] { "10-14 y.o.", "15-19 y.o.", "20+ y.o." };

            // Month and Year columns
            worksheet.Cell(5, 1).Value = monthHeaders[0];
            worksheet.Cell(5, 2).Value = monthHeaders[1];
            worksheet.Range(5, 1, 5, 2).Style.Font.Bold = true;
            worksheet.Range(5, 1, 5, 2).Style.Fill.PatternType = XLFillPatternValues.Solid;
            worksheet.Range(5, 1, 5, 2).Style.Fill.BackgroundColor = XLColor.LightBlue;
            worksheet.Range(5, 1, 5, 2).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            int col = 3;
            foreach (var method in methodHeaders)
            {
                if (method == "TEENAGE PREGNANCIES")
                {
                    // Teenage Pregnancies has no age groups
                    worksheet.Cell(5, col).Value = method;
                    worksheet.Cell(5, col).Style.Font.Bold = true;
                    worksheet.Cell(5, col).Style.Fill.PatternType = XLFillPatternValues.Solid;
                    worksheet.Cell(5, col).Style.Fill.BackgroundColor = XLColor.LightBlue;
                    worksheet.Cell(5, col).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    col++;
                }
                else
                {
                    // Other methods have 3 age groups
                    foreach (var ageGroup in ageGroups)
                    {
                        worksheet.Cell(5, col).Value = $"{method}\n{ageGroup}";
                        worksheet.Cell(5, col).Style.Font.Bold = true;
                        worksheet.Cell(5, col).Style.Fill.PatternType = XLFillPatternValues.Solid;
                        worksheet.Cell(5, col).Style.Fill.BackgroundColor = XLColor.LightBlue;
                        worksheet.Cell(5, col).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        worksheet.Cell(5, col).Style.Alignment.WrapText = true;
                        col++;
                    }
                }
            }

            // Example rows for each month
            var months = new[] { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };
            int row = 6;
            foreach (var month in months)
            {
                worksheet.Cell(row, 1).Value = month;
                worksheet.Cell(row, 2).Value = 2025;
                
                // Fill with zeros for example
                for (int c = 3; c < col; c++)
                {
                    worksheet.Cell(row, c).Value = 0;
                }
                row++;
            }

            // Set column widths
            worksheet.Column(1).Width = 12; // Month
            worksheet.Column(2).Width = 8; // Year
            for (int c = 3; c < col; c++)
            {
                worksheet.Column(c).Width = 10;
            }
            worksheet.SheetView.FreezeRows(5);

            var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            var fileName = $"KRA_Report_Template_{DateTime.Now:yyyyMMdd}.xlsx";
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
    }
}


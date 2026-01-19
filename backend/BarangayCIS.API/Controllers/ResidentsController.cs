using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BarangayCIS.API.Services;
using BarangayCIS.API.Models;
using BarangayCIS.API.Data;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace BarangayCIS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ResidentsController : ControllerBase
    {
        private readonly IResidentService _residentService;
        private readonly ApplicationDbContext _context;

        public ResidentsController(IResidentService residentService, ApplicationDbContext context)
        {
            _residentService = residentService;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            // If pagination parameters are provided, return paginated results
            if (page > 0 && pageSize > 0)
            {
                var pagedResult = await _residentService.GetResidentsPagedAsync(page, pageSize, search);
                return Ok(pagedResult);
            }
            
            // Otherwise, return all results (backward compatibility)
            var residents = await _residentService.GetAllResidentsAsync(search);
            return Ok(residents);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var resident = await _residentService.GetResidentByIdAsync(id);
            if (resident == null) return NotFound();
            return Ok(resident);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Resident resident)
        {
            var created = await _residentService.CreateResidentAsync(resident);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Resident resident)
        {
            var updated = await _residentService.UpdateResidentAsync(id, resident);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id, [FromQuery] bool force = false)
        {
            try
            {
                bool result;
                if (force)
                {
                    result = await _residentService.ForceDeleteResidentAsync(id);
                }
                else
                {
                    result = await _residentService.DeleteResidentAsync(id);
                }
                
                if (!result) return NotFound();
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the resident", error = ex.Message });
            }
        }

        [HttpGet("by-bhw/{bhwProfileId}")]
        public async Task<IActionResult> GetResidentsByBHW(int bhwProfileId, [FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            // If pagination parameters are provided, return paginated results
            if (page > 0 && pageSize > 0)
            {
                var pagedResult = await _residentService.GetResidentsByBHWIdPagedAsync(bhwProfileId, page, pageSize, search);
                return Ok(pagedResult);
            }
            
            // Otherwise, return all results (backward compatibility)
            var residents = await _residentService.GetResidentsByBHWIdAsync(bhwProfileId, search);
            return Ok(residents);
        }

        [HttpGet("bhw/{bhwProfileId}/statistics")]
        public async Task<IActionResult> GetBHWStatistics(int bhwProfileId)
        {
            var statistics = await _residentService.GetBHWResidentStatisticsAsync(bhwProfileId);
            return Ok(statistics);
        }

        [HttpGet("template/download")]
        [AllowAnonymous]
        public IActionResult DownloadResidentTemplate()
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Residents");

            // Add instructions at the top
            worksheet.Cell(1, 1).Value = "RESIDENT DATA TEMPLATE - INSTRUCTIONS";
            worksheet.Cell(1, 1).Style.Font.Bold = true;
            worksheet.Cell(1, 1).Style.Font.FontSize = 14;
            worksheet.Cell(2, 1).Value = "Fields marked with * are required. Delete example rows (rows 7-8) before entering your data.";
            worksheet.Cell(3, 1).Value = "For Yes/No fields, enter 'Yes' or 'No'. For dates, use MM/DD/YYYY format (e.g., 01/15/1990).";
            worksheet.Cell(4, 1).Value = "For Household Name: Enter the Household Number (e.g., HH-001). For Assign BHW: Enter BHW full name (e.g., Juan Dela Cruz).";
            worksheet.Cell(5, 1).Value = ""; // Empty row for spacing

            // Set column headers (starting at row 6) - matching Resident class exactly
            var headers = new[]
            {
                "FirstName*", "LastName*", "MiddleName", "Suffix", "DateOfBirth* (MM/DD/YYYY)",
                "Gender* (Male/Female)", "Address*", "ContactNumber", "Email",
                "CivilStatus", "Occupation", "EmploymentStatus", "IsVoter (Yes/No)",
                "VoterId", "Household Name (HH-Number)", "Assign BHW (Full Name)", "RelationshipToHead", 
                "EducationalAttainment", "BloodType", "IsPWD (Yes/No)", "IsSenior (Yes/No)", "Notes"
            };

            // Set headers with styling
            for (int i = 0; i < headers.Length; i++)
            {
                var cell = worksheet.Cell(6, i + 1);
                cell.Value = headers[i];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.PatternType = XLFillPatternValues.Solid;
                cell.Style.Fill.BackgroundColor = XLColor.LightBlue;
                cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            }

            // Add example rows (matching the header order)
            var exampleRow1 = new object[]
            {
                "Juan", "Dela Cruz", "Santos", "Jr.", "01/15/1990",
                "Male", "123 Main Street, Barangay 5", "09123456789", "juan.delacruz@email.com",
                "Married", "Engineer", "Employed", "Yes",
                "V123456789", "HH-001", "Maria Santos Reyes", "Head", "College",
                "O+", "No", "No", "Sample resident data"
            };

            var exampleRow2 = new object[]
            {
                "Maria", "Santos", "Reyes", "", "03/20/1995",
                "Female", "456 Oak Avenue, Barangay 5", "09987654321", "maria.santos@email.com",
                "Single", "Teacher", "Employed", "Yes",
                "V987654321", "HH-001", "Maria Santos Reyes", "Spouse", "College",
                "A+", "No", "No", ""
            };

            // Add example data (starting at row 7)
            for (int i = 0; i < exampleRow1.Length; i++)
            {
                worksheet.Cell(7, i + 1).Value = exampleRow1[i]?.ToString() ?? "";
                worksheet.Cell(8, i + 1).Value = exampleRow2[i]?.ToString() ?? "";
            }
            
            // Set column widths for better readability
            worksheet.Column(5).Width = 18; // DateOfBirth
            worksheet.Column(7).Width = 30; // Address
            worksheet.Column(8).Width = 15; // ContactNumber
            worksheet.Column(9).Width = 25; // Email
            worksheet.Column(16).Width = 20; // Household Name
            worksheet.Column(17).Width = 25; // Assign BHW
            worksheet.Column(23).Width = 40; // Notes
            worksheet.Columns().AdjustToContents(); // Auto-fit other columns

            // Freeze header row (row 6)
            worksheet.SheetView.FreezeRows(6);

            var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            var fileName = $"Resident_Template_{DateTime.Now:yyyyMMdd}.xlsx";
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        [HttpPost("import")]
        [AllowAnonymous]
        public async Task<IActionResult> ImportResidents([FromForm] IFormFile? file, [FromQuery] string? filePath = null, [FromQuery] string? bhwName = "Emily rotairo")
        {
            try
            {
                Stream fileStream;
                string fileName;

                if (file != null && file.Length > 0)
                {
                    // Use uploaded file
                    fileStream = file.OpenReadStream();
                    fileName = file.FileName;
                }
                else if (!string.IsNullOrWhiteSpace(filePath) && System.IO.File.Exists(filePath))
                {
                    // Use file path (for local development/testing)
                    fileStream = System.IO.File.OpenRead(filePath);
                    fileName = System.IO.Path.GetFileName(filePath);
                }
                else
                {
                    return BadRequest(new { message = "Either a file upload or a valid file path is required" });
                }

                // Find BHW profile by name (flexible matching)
                int? bhwProfileId = null;
                if (!string.IsNullOrWhiteSpace(bhwName))
                {
                    var bhwNameParts = bhwName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (bhwNameParts.Length >= 2)
                    {
                        // Parse name: "Jenny joy J. Name" -> FirstName: "Jenny joy", LastName: "Name"
                        string searchFirstName, searchLastName;
                        if (bhwNameParts.Length >= 4)
                        {
                            // 4+ parts: first two as first name, last as last name
                            searchFirstName = $"{bhwNameParts[0]} {bhwNameParts[1]}".ToLower();
                            searchLastName = bhwNameParts[bhwNameParts.Length - 1].ToLower();
                        }
                        else if (bhwNameParts.Length == 3)
                        {
                            // 3 parts: first two as first name, last as last name
                            searchFirstName = $"{bhwNameParts[0]} {bhwNameParts[1]}".ToLower();
                            searchLastName = bhwNameParts[2].ToLower();
                        }
                        else
                        {
                            // 2 parts: standard first/last
                            searchFirstName = bhwNameParts[0].ToLower();
                            searchLastName = bhwNameParts[1].ToLower();
                        }

                        // Try exact match first
                        var bhwProfile = await _context.BHWProfiles
                            .FirstOrDefaultAsync(b => 
                                b.FirstName.ToLower() == searchFirstName &&
                                b.LastName.ToLower() == searchLastName);

                        // If not found, try flexible matching (first name matches, last name contains or is contained)
                        if (bhwProfile == null)
                        {
                            bhwProfile = await _context.BHWProfiles
                                .FirstOrDefaultAsync(b => 
                                    b.FirstName.ToLower() == searchFirstName &&
                                    (b.LastName.ToLower().Contains(searchLastName) || searchLastName.Contains(b.LastName.ToLower())));
                        }

                        // If still not found, try matching with full name string
                        if (bhwProfile == null)
                        {
                            bhwProfile = await _context.BHWProfiles
                                .FirstOrDefaultAsync(b => 
                                    (b.FirstName.ToLower() + " " + b.LastName.ToLower()).Contains(searchFirstName) &&
                                    (b.FirstName.ToLower() + " " + b.LastName.ToLower()).Contains(searchLastName));
                        }

                        if (bhwProfile != null)
                        {
                            bhwProfileId = bhwProfile.Id;
                        }
                        else
                        {
                            // Auto-create BHW profile if not found
                            var nameParts = bhwName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                            if (nameParts.Length >= 2)
                            {
                                var newBhwNumber = $"BHW-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 6).ToUpper()}";
                                
                                string firstName, middleName, lastName;
                                
                                // Handle name parsing: "Jenny joy J. Name" -> FirstName: "Jenny joy", MiddleName: "J.", LastName: "Name"
                                if (nameParts.Length >= 4)
                                {
                                    // If 4+ parts, take first two as first name, last as last name, second-to-last as middle name
                                    firstName = $"{nameParts[0]} {nameParts[1]}";
                                    middleName = nameParts[nameParts.Length - 2];
                                    lastName = nameParts[nameParts.Length - 1];
                                }
                                else if (nameParts.Length == 3)
                                {
                                    // If 3 parts, take first two as first name, last as last name
                                    firstName = $"{nameParts[0]} {nameParts[1]}";
                                    middleName = null;
                                    lastName = nameParts[2];
                                }
                                else
                                {
                                    // If 2 parts, standard first/last
                                    firstName = nameParts[0];
                                    middleName = null;
                                    lastName = nameParts[1];
                                }
                                
                                var newBhwProfile = new BHWProfile
                                {
                                    FirstName = firstName,
                                    LastName = lastName,
                                    MiddleName = middleName,
                                    DateOfBirth = DateTime.UtcNow.AddYears(-30), // Default age, should be updated
                                    Gender = "Female", // Default, should be updated
                                    Address = "Barangay 5", // Default, should be updated
                                    BHWNumber = newBhwNumber,
                                    DateAppointed = DateTime.UtcNow,
                                    Status = "Active",
                                    CreatedAt = DateTime.UtcNow
                                };
                                
                                _context.BHWProfiles.Add(newBhwProfile);
                                await _context.SaveChangesAsync();
                                bhwProfileId = newBhwProfile.Id;
                            }
                        }
                    }
                }

                if (!bhwProfileId.HasValue)
                {
                    return BadRequest(new { message = $"BHW profile '{bhwName}' could not be created. Please ensure the BHW name is in 'FirstName LastName' format." });
                }

                // Read Excel file
                using var workbook = new XLWorkbook(fileStream);
                var worksheet = workbook.Worksheets.First();

                var importedCount = 0;
                var errorCount = 0;
                var errors = new List<string>();

                // Find header row (look for "FirstName" or similar)
                int headerRow = 1;
                for (int row = 1; row <= 10; row++)
                {
                    var cellValue = worksheet.Cell(row, 1).GetString().Trim();
                    if (cellValue.Contains("FirstName", StringComparison.OrdinalIgnoreCase) || 
                        cellValue.Contains("First Name", StringComparison.OrdinalIgnoreCase))
                    {
                        headerRow = row;
                        break;
                    }
                }

                // Create column mapping
                var columnMap = new Dictionary<string, int>();
                for (int col = 1; col <= worksheet.LastColumnUsed().ColumnNumber(); col++)
                {
                    var headerValue = worksheet.Cell(headerRow, col).GetString().Trim().ToLower();
                    if (headerValue.Contains("firstname") || headerValue.Contains("first name")) columnMap["FirstName"] = col;
                    else if (headerValue.Contains("lastname") || headerValue.Contains("last name")) columnMap["LastName"] = col;
                    else if (headerValue.Contains("middlename") || headerValue.Contains("middle name")) columnMap["MiddleName"] = col;
                    else if (headerValue.Contains("suffix")) columnMap["Suffix"] = col;
                    else if (headerValue.Contains("dateofbirth") || headerValue.Contains("date of birth") || headerValue.Contains("birthdate")) columnMap["DateOfBirth"] = col;
                    else if (headerValue.Contains("gender")) columnMap["Gender"] = col;
                    else if (headerValue.Contains("address")) columnMap["Address"] = col;
                    else if (headerValue.Contains("contact") || headerValue.Contains("phone")) columnMap["ContactNumber"] = col;
                    else if (headerValue.Contains("email")) columnMap["Email"] = col;
                    else if (headerValue.Contains("civil") || headerValue.Contains("status")) columnMap["CivilStatus"] = col;
                    else if (headerValue.Contains("occupation")) columnMap["Occupation"] = col;
                    else if (headerValue.Contains("employment")) columnMap["EmploymentStatus"] = col;
                    else if (headerValue.Contains("voter") && !headerValue.Contains("id")) columnMap["IsVoter"] = col;
                    else if (headerValue.Contains("voterid") || headerValue.Contains("voter id")) columnMap["VoterId"] = col;
                    else if (headerValue.Contains("household")) columnMap["Household"] = col;
                    else if (headerValue.Contains("relationship")) columnMap["RelationshipToHead"] = col;
                    else if (headerValue.Contains("educational") || headerValue.Contains("education")) columnMap["EducationalAttainment"] = col;
                    else if (headerValue.Contains("blood")) columnMap["BloodType"] = col;
                    else if (headerValue.Contains("pwd") || headerValue.Contains("person with disability")) columnMap["IsPWD"] = col;
                    else if (headerValue.Contains("senior")) columnMap["IsSenior"] = col;
                    else if (headerValue.Contains("notes") || headerValue.Contains("note")) columnMap["Notes"] = col;
                }

                // Validate required columns
                if (!columnMap.ContainsKey("FirstName") || !columnMap.ContainsKey("LastName") || 
                    !columnMap.ContainsKey("DateOfBirth") || 
                    !columnMap.ContainsKey("Address"))
                {
                    return BadRequest(new { message = "Required columns not found. Please ensure the Excel file has: FirstName, LastName, DateOfBirth, and Address columns. Gender is optional and will default to 'Male' if not provided." });
                }

                // Process data rows
                for (int row = headerRow + 1; row <= worksheet.LastRowUsed().RowNumber(); row++)
                {
                    try
                    {
                        var firstName = worksheet.Cell(row, columnMap["FirstName"]).GetString().Trim();
                        var lastName = worksheet.Cell(row, columnMap["LastName"]).GetString().Trim();

                        // Skip empty rows
                        if (string.IsNullOrWhiteSpace(firstName) && string.IsNullOrWhiteSpace(lastName))
                        {
                            continue;
                        }

                        if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
                        {
                            errors.Add($"Row {row}: FirstName and LastName are required");
                            errorCount++;
                            continue;
                        }

                        // Parse date of birth
                        DateTime dateOfBirth = DateTime.MinValue;
                        var dobValue = worksheet.Cell(row, columnMap["DateOfBirth"]).GetString().Trim();
                        if (string.IsNullOrWhiteSpace(dobValue))
                        {
                            errors.Add($"Row {row}: DateOfBirth is required");
                            errorCount++;
                            continue;
                        }

                        // Try multiple date parsing strategies
                        bool dateParsed = false;
                        
                        // Fix common date format issues
                        var fixedDobValue = dobValue;
                        
                        // Remove time components (e.g., "01/10/1993 12:00:00 am" -> "01/10/1993")
                        if (fixedDobValue.Contains(" ") && fixedDobValue.Contains(":"))
                        {
                            var spaceIndex = fixedDobValue.IndexOf(" ");
                            fixedDobValue = fixedDobValue.Substring(0, spaceIndex).Trim();
                        }
                        
                        // Remove extra spaces
                        fixedDobValue = fixedDobValue.Replace(" ", "").Trim();
                        
                        // Fix dates with spaces like "3/30/19 75" -> "3/30/1975" or "8/31/ 2024" -> "8/31/2024"
                        fixedDobValue = System.Text.RegularExpressions.Regex.Replace(fixedDobValue, @"(\d+)\s+(\d+)", "$1$2");
                        
                        // Fix malformed dates like "03/301983" -> "03/30/1983"
                        if (fixedDobValue.Length == 9 && fixedDobValue.Contains("/") && !fixedDobValue.Contains("//"))
                        {
                            // Pattern: M/DDYYYY or MM/DDYYYY
                            var parts = fixedDobValue.Split('/');
                            if (parts.Length == 2 && parts[1].Length >= 6)
                            {
                                var month = parts[0];
                                var rest = parts[1];
                                if (rest.Length == 6)
                                {
                                    // MM/DDYYYY format
                                    fixedDobValue = $"{month}/{rest.Substring(0, 2)}/{rest.Substring(2)}";
                                }
                            }
                        }
                        
                        // Fix dates with extra digits in year like "12/23/19893" -> try to fix
                        if (fixedDobValue.Contains("/"))
                        {
                            var parts = fixedDobValue.Split('/');
                            if (parts.Length == 3 && parts[2].Length > 4)
                            {
                                // Year has extra digits, try to extract valid 4-digit year
                                var yearStr = parts[2];
                                if (yearStr.Length == 5)
                                {
                                    // Try both possibilities: first 4 digits or last 4 digits
                                    var year1 = yearStr.Substring(0, 4);
                                    var year2 = yearStr.Substring(1, 4);
                                    if (int.TryParse(year1, out var y1) && y1 >= 1900 && y1 <= 2100)
                                    {
                                        fixedDobValue = $"{parts[0]}/{parts[1]}/{year1}";
                                    }
                                    else if (int.TryParse(year2, out var y2) && y2 >= 1900 && y2 <= 2100)
                                    {
                                        fixedDobValue = $"{parts[0]}/{parts[1]}/{year2}";
                                    }
                                }
                            }
                        }
                        
                        // Fix invalid month like "112/9/2021" -> try "11/2/2021" or "1/12/2021"
                        if (fixedDobValue.Contains("/"))
                        {
                            var parts = fixedDobValue.Split('/');
                            if (parts.Length == 3)
                            {
                                if (int.TryParse(parts[0], out var month) && month > 12)
                                {
                                    // Month is invalid, try to split it
                                    var monthStr = parts[0];
                                    if (monthStr.Length == 3)
                                    {
                                        // Try splitting as 1/12 or 11/2
                                        var try1 = $"{monthStr.Substring(0, 1)}/{monthStr.Substring(1)}/{parts[2]}";
                                        var try2 = $"{monthStr.Substring(0, 2)}/{monthStr.Substring(2)}/{parts[2]}";
                                        
                                        // Check which one is valid
                                        if (DateTime.TryParse(try1, out var dt1) && dt1.Month <= 12 && dt1.Day <= 31)
                                        {
                                            fixedDobValue = try1;
                                        }
                                        else if (DateTime.TryParse(try2, out var dt2) && dt2.Month <= 12 && dt2.Day <= 31)
                                        {
                                            fixedDobValue = try2;
                                        }
                                    }
                                }
                            }
                        }
                        
                        // Fix 2-digit years (assume 20xx for years 00-50, 19xx for years 51-99)
                        if (fixedDobValue.Contains("/"))
                        {
                            var parts = fixedDobValue.Split('/');
                            if (parts.Length == 3 && parts[2].Length == 2)
                            {
                                if (int.TryParse(parts[2], out var year))
                                {
                                    var fullYear = year <= 50 ? 2000 + year : 1900 + year;
                                    fixedDobValue = $"{parts[0]}/{parts[1]}/{fullYear}";
                                }
                            }
                        }
                        
                        // First, check for invalid Feb 29 dates before parsing
                        if (fixedDobValue.Contains("/"))
                        {
                            var parts = fixedDobValue.Split('/');
                            if (parts.Length == 3)
                            {
                                if (int.TryParse(parts[0], out var month) && 
                                    int.TryParse(parts[1], out var day) && 
                                    int.TryParse(parts[2], out var year) &&
                                    month >= 1 && month <= 12 && day >= 1 && day <= 31 && year >= 1900 && year <= 2100)
                                {
                                    // Check for invalid Feb 29 in non-leap year
                                    if (month == 2 && day == 29)
                                    {
                                        bool isLeapYear = (year % 4 == 0 && year % 100 != 0) || (year % 400 == 0);
                                        if (!isLeapYear)
                                        {
                                            // Not a leap year, adjust to Feb 28
                                            day = 28;
                                            fixedDobValue = $"{month}/{day}/{year}";
                                        }
                                    }
                                    
                                    // Try to create the date directly
                                    try
                                    {
                                        dateOfBirth = new DateTime(year, month, day);
                                        dateParsed = true;
                                    }
                                    catch
                                    {
                                        // If direct creation fails, try parsing
                                    }
                                }
                            }
                        }
                        
                        // If not parsed yet, try standard DateTime parsing
                        if (!dateParsed && DateTime.TryParse(fixedDobValue, out dateOfBirth))
                        {
                            // Validate the date is actually valid (e.g., not Feb 29 in non-leap year)
                            if (dateOfBirth.Month == 2 && dateOfBirth.Day == 29)
                            {
                                // Check if it's a leap year
                                var year = dateOfBirth.Year;
                                bool isLeapYear = (year % 4 == 0 && year % 100 != 0) || (year % 400 == 0);
                                if (!isLeapYear)
                                {
                                    // Not a leap year, adjust to Feb 28
                                    dateOfBirth = new DateTime(year, 2, 28);
                                }
                            }
                            dateParsed = true;
                        }
                        // Try parsing as Excel date number
                        else if (double.TryParse(fixedDobValue, out var excelDate))
                        {
                            try
                            {
                                dateOfBirth = DateTime.FromOADate(excelDate);
                                dateParsed = true;
                            }
                            catch { }
                        }
                        // Try parsing with common date formats
                        else
                        {
                            var dateFormats = new[] { "M/d/yyyy", "MM/dd/yyyy", "M/dd/yyyy", "MM/d/yyyy", 
                                                       "d/M/yyyy", "dd/MM/yyyy", "yyyy-MM-dd", "yyyy/MM/dd" };
                            foreach (var format in dateFormats)
                            {
                                if (DateTime.TryParseExact(fixedDobValue, format, System.Globalization.CultureInfo.InvariantCulture, 
                                    System.Globalization.DateTimeStyles.None, out dateOfBirth))
                                {
                                    dateParsed = true;
                                    break;
                                }
                            }
                        }

                        if (!dateParsed)
                        {
                            errors.Add($"Row {row}: Invalid DateOfBirth format: {dobValue}");
                            errorCount++;
                            continue;
                        }
                        
                        // Ensure dateOfBirth is valid before proceeding
                        if (dateOfBirth == DateTime.MinValue)
                        {
                            errors.Add($"Row {row}: DateOfBirth could not be parsed: {dobValue}");
                            errorCount++;
                            continue;
                        }

                        // Get gender
                        string gender = null;
                        if (columnMap.ContainsKey("Gender"))
                        {
                            gender = worksheet.Cell(row, columnMap["Gender"]).GetString().Trim();
                        }
                        
                        // If gender is missing or empty, try to infer from name or use default
                        if (string.IsNullOrWhiteSpace(gender))
                        {
                            // Try to infer from common name patterns or use default
                            // For now, use "Male" as default (you can change this logic)
                            gender = "Male"; // Default gender if not specified
                        }

                        // Normalize gender
                        if (gender.Equals("M", StringComparison.OrdinalIgnoreCase) || 
                            gender.Equals("Male", StringComparison.OrdinalIgnoreCase))
                        {
                            gender = "Male";
                        }
                        else if (gender.Equals("F", StringComparison.OrdinalIgnoreCase) || 
                                 gender.Equals("Female", StringComparison.OrdinalIgnoreCase))
                        {
                            gender = "Female";
                        }
                        else
                        {
                            // If gender doesn't match known values, default to "Male"
                            gender = "Male";
                        }

                        // Get address
                        var address = worksheet.Cell(row, columnMap["Address"]).GetString().Trim();
                        if (string.IsNullOrWhiteSpace(address))
                        {
                            // Use default address if missing
                            address = "San Juan St. Barangay 5";
                        }

                    // Check for duplicate (by FirstName, LastName, and DateOfBirth)
                    var existingResident = await _context.Residents
                        .FirstOrDefaultAsync(r => 
                            r.FirstName.ToLower() == firstName.ToLower() &&
                            r.LastName.ToLower() == lastName.ToLower() &&
                            r.DateOfBirth.Date == dateOfBirth.Date);

                    if (existingResident != null)
                    {
                        // Skip duplicate, but update BHW assignment if not set
                        if (!existingResident.BHWProfileId.HasValue && bhwProfileId.HasValue)
                        {
                            existingResident.BHWProfileId = bhwProfileId.Value;
                            existingResident.UpdatedAt = DateTime.UtcNow;
                            await _context.SaveChangesAsync();
                            importedCount++; // Count as imported since we updated it
                        }
                        else
                        {
                            // Already exists, skip
                            continue;
                        }
                    }
                    else
                    {
                        // Create new resident
                        var resident = new Resident
                        {
                            FirstName = firstName,
                            LastName = lastName,
                            MiddleName = columnMap.ContainsKey("MiddleName") ? worksheet.Cell(row, columnMap["MiddleName"]).GetString().Trim() : null,
                            Suffix = columnMap.ContainsKey("Suffix") ? worksheet.Cell(row, columnMap["Suffix"]).GetString().Trim() : null,
                            DateOfBirth = dateOfBirth,
                            Gender = gender,
                            Address = address,
                            ContactNumber = columnMap.ContainsKey("ContactNumber") ? worksheet.Cell(row, columnMap["ContactNumber"]).GetString().Trim() : null,
                            Email = columnMap.ContainsKey("Email") ? worksheet.Cell(row, columnMap["Email"]).GetString().Trim() : null,
                            CivilStatus = columnMap.ContainsKey("CivilStatus") ? worksheet.Cell(row, columnMap["CivilStatus"]).GetString().Trim() : null,
                            Occupation = columnMap.ContainsKey("Occupation") ? worksheet.Cell(row, columnMap["Occupation"]).GetString().Trim() : null,
                            EmploymentStatus = columnMap.ContainsKey("EmploymentStatus") ? worksheet.Cell(row, columnMap["EmploymentStatus"]).GetString().Trim() : null,
                            IsVoter = columnMap.ContainsKey("IsVoter") ? 
                                worksheet.Cell(row, columnMap["IsVoter"]).GetString().Trim().Equals("Yes", StringComparison.OrdinalIgnoreCase) : false,
                            VoterId = columnMap.ContainsKey("VoterId") ? worksheet.Cell(row, columnMap["VoterId"]).GetString().Trim() : null,
                            RelationshipToHead = columnMap.ContainsKey("RelationshipToHead") ? worksheet.Cell(row, columnMap["RelationshipToHead"]).GetString().Trim() : null,
                            EducationalAttainment = columnMap.ContainsKey("EducationalAttainment") ? worksheet.Cell(row, columnMap["EducationalAttainment"]).GetString().Trim() : null,
                            BloodType = columnMap.ContainsKey("BloodType") ? worksheet.Cell(row, columnMap["BloodType"]).GetString().Trim() : null,
                            IsPWD = columnMap.ContainsKey("IsPWD") ? 
                                worksheet.Cell(row, columnMap["IsPWD"]).GetString().Trim().Equals("Yes", StringComparison.OrdinalIgnoreCase) : false,
                            IsSenior = columnMap.ContainsKey("IsSenior") ? 
                                worksheet.Cell(row, columnMap["IsSenior"]).GetString().Trim().Equals("Yes", StringComparison.OrdinalIgnoreCase) : false,
                            Notes = columnMap.ContainsKey("Notes") ? worksheet.Cell(row, columnMap["Notes"]).GetString().Trim() : null,
                            BHWProfileId = bhwProfileId.Value,
                            CreatedAt = DateTime.UtcNow
                        };

                        // Handle household lookup
                        if (columnMap.ContainsKey("Household"))
                        {
                            var householdValue = worksheet.Cell(row, columnMap["Household"]).GetString().Trim();
                            if (!string.IsNullOrWhiteSpace(householdValue))
                            {
                                var household = await _context.Households
                                    .FirstOrDefaultAsync(h => h.HouseholdNumber.ToLower() == householdValue.ToLower());
                                
                                if (household != null)
                                {
                                    resident.HouseholdId = household.Id;
                                }
                            }
                        }

                        await _residentService.CreateResidentAsync(resident);
                        importedCount++;
                    }
                    }
                    catch (Exception ex)
                    {
                        errorCount++;
                        errors.Add($"Row {row}: {ex.Message}");
                    }
                }

                return Ok(new
                {
                    message = $"Import completed. Imported: {importedCount}, Errors: {errorCount}",
                    importedCount,
                    errorCount,
                    errors = errors.Take(50).ToList() // Limit to first 50 errors
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error importing residents", error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        [HttpPost("import-from-downloads")]
        [AllowAnonymous]
        public async Task<IActionResult> ImportResidentsFromDownloads([FromQuery] string? bhwName = "Emily rotairo", [FromQuery] string? downloadsPath = null)
        {
            try
            {
                // Get Downloads folder path
                string downloadsFolder;
                if (!string.IsNullOrWhiteSpace(downloadsPath) && System.IO.Directory.Exists(downloadsPath))
                {
                    downloadsFolder = downloadsPath;
                }
                else
                {
                    // Default to user's Downloads folder
                    downloadsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
                }

                if (!System.IO.Directory.Exists(downloadsFolder))
                {
                    return BadRequest(new { message = $"Downloads folder not found: {downloadsFolder}" });
                }

                // Find all Excel files in Downloads folder
                var excelFiles = System.IO.Directory.GetFiles(downloadsFolder, "*.xlsx", SearchOption.TopDirectoryOnly)
                    .Concat(System.IO.Directory.GetFiles(downloadsFolder, "*.xls", SearchOption.TopDirectoryOnly))
                    .ToList();

                if (excelFiles.Count == 0)
                {
                    return Ok(new
                    {
                        message = "No Excel files found in Downloads folder",
                        filesProcessed = 0,
                        totalImported = 0,
                        totalErrors = 0,
                        results = new List<object>()
                    });
                }

                // Find BHW profile by name (flexible matching)
                int? bhwProfileId = null;
                if (!string.IsNullOrWhiteSpace(bhwName))
                {
                    var bhwNameParts = bhwName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (bhwNameParts.Length >= 2)
                    {
                        // Parse name: "Jenny joy J. Name" -> FirstName: "Jenny joy", LastName: "Name"
                        string searchFirstName, searchLastName;
                        if (bhwNameParts.Length >= 4)
                        {
                            // 4+ parts: first two as first name, last as last name
                            searchFirstName = $"{bhwNameParts[0]} {bhwNameParts[1]}".ToLower();
                            searchLastName = bhwNameParts[bhwNameParts.Length - 1].ToLower();
                        }
                        else if (bhwNameParts.Length == 3)
                        {
                            // 3 parts: first two as first name, last as last name
                            searchFirstName = $"{bhwNameParts[0]} {bhwNameParts[1]}".ToLower();
                            searchLastName = bhwNameParts[2].ToLower();
                        }
                        else
                        {
                            // 2 parts: standard first/last
                            searchFirstName = bhwNameParts[0].ToLower();
                            searchLastName = bhwNameParts[1].ToLower();
                        }

                        // Try exact match first
                        var bhwProfile = await _context.BHWProfiles
                            .FirstOrDefaultAsync(b => 
                                b.FirstName.ToLower() == searchFirstName &&
                                b.LastName.ToLower() == searchLastName);

                        // If not found, try flexible matching (first name matches, last name contains or is contained)
                        if (bhwProfile == null)
                        {
                            bhwProfile = await _context.BHWProfiles
                                .FirstOrDefaultAsync(b => 
                                    b.FirstName.ToLower() == searchFirstName &&
                                    (b.LastName.ToLower().Contains(searchLastName) || searchLastName.Contains(b.LastName.ToLower())));
                        }

                        // If still not found, try matching with full name string
                        if (bhwProfile == null)
                        {
                            bhwProfile = await _context.BHWProfiles
                                .FirstOrDefaultAsync(b => 
                                    (b.FirstName.ToLower() + " " + b.LastName.ToLower()).Contains(searchFirstName) &&
                                    (b.FirstName.ToLower() + " " + b.LastName.ToLower()).Contains(searchLastName));
                        }

                        if (bhwProfile != null)
                        {
                            bhwProfileId = bhwProfile.Id;
                        }
                        else
                        {
                            // Auto-create BHW profile if not found
                            var nameParts = bhwName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                            if (nameParts.Length >= 2)
                            {
                                var newBhwNumber = $"BHW-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 6).ToUpper()}";
                                
                                string firstName, middleName, lastName;
                                
                                // Handle name parsing: "Jenny joy J. Name" -> FirstName: "Jenny joy", MiddleName: "J.", LastName: "Name"
                                if (nameParts.Length >= 4)
                                {
                                    // If 4+ parts, take first two as first name, last as last name, second-to-last as middle name
                                    firstName = $"{nameParts[0]} {nameParts[1]}";
                                    middleName = nameParts[nameParts.Length - 2];
                                    lastName = nameParts[nameParts.Length - 1];
                                }
                                else if (nameParts.Length == 3)
                                {
                                    // If 3 parts, take first two as first name, last as last name
                                    firstName = $"{nameParts[0]} {nameParts[1]}";
                                    middleName = null;
                                    lastName = nameParts[2];
                                }
                                else
                                {
                                    // If 2 parts, standard first/last
                                    firstName = nameParts[0];
                                    middleName = null;
                                    lastName = nameParts[1];
                                }
                                
                                var newBhwProfile = new BHWProfile
                                {
                                    FirstName = firstName,
                                    LastName = lastName,
                                    MiddleName = middleName,
                                    DateOfBirth = DateTime.UtcNow.AddYears(-30), // Default age, should be updated
                                    Gender = "Female", // Default, should be updated
                                    Address = "Barangay 5", // Default, should be updated
                                    BHWNumber = newBhwNumber,
                                    DateAppointed = DateTime.UtcNow,
                                    Status = "Active",
                                    CreatedAt = DateTime.UtcNow
                                };
                                
                                _context.BHWProfiles.Add(newBhwProfile);
                                await _context.SaveChangesAsync();
                                bhwProfileId = newBhwProfile.Id;
                            }
                        }
                    }
                }

                if (!bhwProfileId.HasValue)
                {
                    return BadRequest(new { message = $"BHW profile '{bhwName}' could not be created. Please ensure the BHW name is in 'FirstName LastName' format." });
                }

                var results = new List<object>();
                int totalImported = 0;
                int totalErrors = 0;

                // Process each Excel file
                foreach (var filePath in excelFiles)
                {
                    var fileName = Path.GetFileName(filePath);
                    var fileResult = new
                    {
                        fileName = fileName,
                        filePath = filePath,
                        importedCount = 0,
                        errorCount = 0,
                        errors = new List<string>()
                    };

                    try
                    {
                        using var fileStream = System.IO.File.OpenRead(filePath);
                        using var workbook = new XLWorkbook(fileStream);
                        var worksheet = workbook.Worksheets.First();

                        var importedCount = 0;
                        var errorCount = 0;
                        var errors = new List<string>();

                        // Find header row
                        int headerRow = 1;
                        for (int row = 1; row <= 10; row++)
                        {
                            var cellValue = worksheet.Cell(row, 1).GetString().Trim();
                            if (cellValue.Contains("FirstName", StringComparison.OrdinalIgnoreCase) ||
                                cellValue.Contains("First Name", StringComparison.OrdinalIgnoreCase))
                            {
                                headerRow = row;
                                break;
                            }
                        }

                        // Create column mapping (reuse the same logic from ImportResidents)
                        var columnMap = new Dictionary<string, int>();
                        for (int col = 1; col <= worksheet.LastColumnUsed().ColumnNumber(); col++)
                        {
                            var headerValue = worksheet.Cell(headerRow, col).GetString().Trim().ToLower();
                            if (headerValue.Contains("firstname") || headerValue.Contains("first name")) columnMap["FirstName"] = col;
                            else if (headerValue.Contains("lastname") || headerValue.Contains("last name")) columnMap["LastName"] = col;
                            else if (headerValue.Contains("middlename") || headerValue.Contains("middle name")) columnMap["MiddleName"] = col;
                            else if (headerValue.Contains("suffix")) columnMap["Suffix"] = col;
                            else if (headerValue.Contains("dateofbirth") || headerValue.Contains("date of birth") || headerValue.Contains("birthdate")) columnMap["DateOfBirth"] = col;
                            else if (headerValue.Contains("gender")) columnMap["Gender"] = col;
                            else if (headerValue.Contains("address")) columnMap["Address"] = col;
                            else if (headerValue.Contains("contact") || headerValue.Contains("phone")) columnMap["ContactNumber"] = col;
                            else if (headerValue.Contains("email")) columnMap["Email"] = col;
                            else if (headerValue.Contains("civil") || headerValue.Contains("status")) columnMap["CivilStatus"] = col;
                            else if (headerValue.Contains("occupation")) columnMap["Occupation"] = col;
                            else if (headerValue.Contains("employment")) columnMap["EmploymentStatus"] = col;
                            else if (headerValue.Contains("voter") && !headerValue.Contains("id")) columnMap["IsVoter"] = col;
                            else if (headerValue.Contains("voterid") || headerValue.Contains("voter id")) columnMap["VoterId"] = col;
                            else if (headerValue.Contains("household")) columnMap["Household"] = col;
                            else if (headerValue.Contains("relationship")) columnMap["RelationshipToHead"] = col;
                            else if (headerValue.Contains("educational") || headerValue.Contains("education")) columnMap["EducationalAttainment"] = col;
                            else if (headerValue.Contains("blood")) columnMap["BloodType"] = col;
                            else if (headerValue.Contains("pwd") || headerValue.Contains("person with disability")) columnMap["IsPWD"] = col;
                            else if (headerValue.Contains("senior")) columnMap["IsSenior"] = col;
                            else if (headerValue.Contains("notes") || headerValue.Contains("note")) columnMap["Notes"] = col;
                        }

                        // Validate required columns
                        if (!columnMap.ContainsKey("FirstName") || !columnMap.ContainsKey("LastName") ||
                            !columnMap.ContainsKey("DateOfBirth") ||
                            !columnMap.ContainsKey("Address"))
                        {
                            errors.Add("Required columns not found. Skipping file.");
                            errorCount++;
                            results.Add(new
                            {
                                fileName = fileName,
                                filePath = filePath,
                                importedCount = 0,
                                errorCount = errorCount,
                                errors = errors
                            });
                            totalErrors += errorCount;
                            continue;
                        }

                        // Process data rows (reuse the same logic from ImportResidents)
                        for (int row = headerRow + 1; row <= worksheet.LastRowUsed().RowNumber(); row++)
                        {
                            try
                            {
                                var firstName = worksheet.Cell(row, columnMap["FirstName"]).GetString().Trim();
                                var lastName = worksheet.Cell(row, columnMap["LastName"]).GetString().Trim();

                                if (string.IsNullOrWhiteSpace(firstName) && string.IsNullOrWhiteSpace(lastName))
                                    continue;

                                if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
                                {
                                    errors.Add($"Row {row}: FirstName and LastName are required");
                                    errorCount++;
                                    continue;
                                }

                                // Parse date of birth (using comprehensive logic from single file import)
                                DateTime dateOfBirth = DateTime.MinValue;
                                var dobValue = worksheet.Cell(row, columnMap["DateOfBirth"]).GetString().Trim();
                                if (string.IsNullOrWhiteSpace(dobValue))
                                {
                                    errors.Add($"Row {row}: DateOfBirth is required");
                                    errorCount++;
                                    continue;
                                }

                                // Try multiple date parsing strategies
                                bool dateParsed = false;
                                
                                // Fix common date format issues
                                var fixedDobValue = dobValue;
                                
                                // Remove time components (e.g., "01/10/1993 12:00:00 am" -> "01/10/1993")
                                if (fixedDobValue.Contains(" ") && fixedDobValue.Contains(":"))
                                {
                                    var spaceIndex = fixedDobValue.IndexOf(" ");
                                    fixedDobValue = fixedDobValue.Substring(0, spaceIndex).Trim();
                                }
                                
                                // Remove extra spaces
                                fixedDobValue = fixedDobValue.Replace(" ", "").Trim();
                                
                                // Fix dates with spaces like "3/30/19 75" -> "3/30/1975" or "8/31/ 2024" -> "8/31/2024"
                                fixedDobValue = System.Text.RegularExpressions.Regex.Replace(fixedDobValue, @"(\d+)\s+(\d+)", "$1$2");
                                
                                // Fix malformed dates like "03/301983" -> "03/30/1983"
                                if (fixedDobValue.Length == 9 && fixedDobValue.Contains("/") && !fixedDobValue.Contains("//"))
                                {
                                    var parts = fixedDobValue.Split('/');
                                    if (parts.Length == 2 && parts[1].Length >= 6)
                                    {
                                        var month = parts[0];
                                        var rest = parts[1];
                                        if (rest.Length == 6)
                                        {
                                            fixedDobValue = $"{month}/{rest.Substring(0, 2)}/{rest.Substring(2)}";
                                        }
                                    }
                                }
                                
                                // Fix dates with extra digits in year
                                if (fixedDobValue.Contains("/"))
                                {
                                    var parts = fixedDobValue.Split('/');
                                    if (parts.Length == 3 && parts[2].Length > 4)
                                    {
                                        var yearStr = parts[2];
                                        if (yearStr.Length == 5)
                                        {
                                            var year1 = yearStr.Substring(0, 4);
                                            var year2 = yearStr.Substring(1, 4);
                                            if (int.TryParse(year1, out var y1) && y1 >= 1900 && y1 <= 2100)
                                            {
                                                fixedDobValue = $"{parts[0]}/{parts[1]}/{year1}";
                                            }
                                            else if (int.TryParse(year2, out var y2) && y2 >= 1900 && y2 <= 2100)
                                            {
                                                fixedDobValue = $"{parts[0]}/{parts[1]}/{year2}";
                                            }
                                        }
                                    }
                                }
                                
                                // Fix invalid month like "112/9/2021" -> try "11/2/2021" or "1/12/2021"
                                if (fixedDobValue.Contains("/"))
                                {
                                    var parts = fixedDobValue.Split('/');
                                    if (parts.Length == 3)
                                    {
                                        if (int.TryParse(parts[0], out var month) && month > 12)
                                        {
                                            var monthStr = parts[0];
                                            if (monthStr.Length == 3)
                                            {
                                                var try1 = $"{monthStr.Substring(0, 1)}/{monthStr.Substring(1)}/{parts[2]}";
                                                var try2 = $"{monthStr.Substring(0, 2)}/{monthStr.Substring(2)}/{parts[2]}";
                                                
                                                if (DateTime.TryParse(try1, out var dt1) && dt1.Month <= 12 && dt1.Day <= 31)
                                                {
                                                    fixedDobValue = try1;
                                                }
                                                else if (DateTime.TryParse(try2, out var dt2) && dt2.Month <= 12 && dt2.Day <= 31)
                                                {
                                                    fixedDobValue = try2;
                                                }
                                            }
                                        }
                                    }
                                }
                                
                                // Fix 2-digit years (assume 20xx for years 00-50, 19xx for years 51-99)
                                if (fixedDobValue.Contains("/"))
                                {
                                    var parts = fixedDobValue.Split('/');
                                    if (parts.Length == 3 && parts[2].Length == 2)
                                    {
                                        if (int.TryParse(parts[2], out var year))
                                        {
                                            var fullYear = year <= 50 ? 2000 + year : 1900 + year;
                                            fixedDobValue = $"{parts[0]}/{parts[1]}/{fullYear}";
                                        }
                                    }
                                }
                                
                                // Check for invalid Feb 29 dates before parsing
                                if (fixedDobValue.Contains("/"))
                                {
                                    var parts = fixedDobValue.Split('/');
                                    if (parts.Length == 3)
                                    {
                                        if (int.TryParse(parts[0], out var month) && 
                                            int.TryParse(parts[1], out var day) && 
                                            int.TryParse(parts[2], out var year) &&
                                            month >= 1 && month <= 12 && day >= 1 && day <= 31 && year >= 1900 && year <= 2100)
                                        {
                                            if (month == 2 && day == 29)
                                            {
                                                bool isLeapYear = (year % 4 == 0 && year % 100 != 0) || (year % 400 == 0);
                                                if (!isLeapYear)
                                                {
                                                    day = 28;
                                                    fixedDobValue = $"{month}/{day}/{year}";
                                                }
                                            }
                                            
                                            try
                                            {
                                                dateOfBirth = new DateTime(year, month, day);
                                                dateParsed = true;
                                            }
                                            catch { }
                                        }
                                    }
                                }
                                
                                // If not parsed yet, try standard DateTime parsing
                                if (!dateParsed && DateTime.TryParse(fixedDobValue, out dateOfBirth))
                                {
                                    if (dateOfBirth.Month == 2 && dateOfBirth.Day == 29)
                                    {
                                        var year = dateOfBirth.Year;
                                        bool isLeapYear = (year % 4 == 0 && year % 100 != 0) || (year % 400 == 0);
                                        if (!isLeapYear)
                                        {
                                            dateOfBirth = new DateTime(year, 2, 28);
                                        }
                                    }
                                    dateParsed = true;
                                }
                                // Try parsing as Excel date number
                                else if (!dateParsed && double.TryParse(fixedDobValue, out var excelDate))
                                {
                                    try
                                    {
                                        dateOfBirth = DateTime.FromOADate(excelDate);
                                        dateParsed = true;
                                    }
                                    catch { }
                                }
                                // Try parsing with common date formats
                                else if (!dateParsed)
                                {
                                    var dateFormats = new[] { "M/d/yyyy", "MM/dd/yyyy", "M/dd/yyyy", "MM/d/yyyy", 
                                                               "d/M/yyyy", "dd/MM/yyyy", "yyyy-MM-dd", "yyyy/MM/dd" };
                                    foreach (var format in dateFormats)
                                    {
                                        if (DateTime.TryParseExact(fixedDobValue, format, System.Globalization.CultureInfo.InvariantCulture, 
                                            System.Globalization.DateTimeStyles.None, out dateOfBirth))
                                        {
                                            dateParsed = true;
                                            break;
                                        }
                                    }
                                }

                                if (!dateParsed || dateOfBirth == DateTime.MinValue)
                                {
                                    errors.Add($"Row {row}: Invalid DateOfBirth format: {dobValue}");
                                    errorCount++;
                                    continue;
                                }

                                // Get gender
                                string gender = null;
                                if (columnMap.ContainsKey("Gender"))
                                {
                                    gender = worksheet.Cell(row, columnMap["Gender"]).GetString().Trim();
                                }
                                
                                // If gender is missing or empty, use default
                                if (string.IsNullOrWhiteSpace(gender))
                                {
                                    gender = "Male"; // Default gender if not specified
                                }

                                // Normalize gender
                                if (gender.Equals("M", StringComparison.OrdinalIgnoreCase) || 
                                    gender.Equals("Male", StringComparison.OrdinalIgnoreCase))
                                {
                                    gender = "Male";
                                }
                                else if (gender.Equals("F", StringComparison.OrdinalIgnoreCase) || 
                                         gender.Equals("Female", StringComparison.OrdinalIgnoreCase))
                                {
                                    gender = "Female";
                                }
                                else
                                {
                                    // If gender doesn't match known values, default to "Male"
                                    gender = "Male";
                                }

                                var address = worksheet.Cell(row, columnMap["Address"]).GetString().Trim();
                                if (string.IsNullOrWhiteSpace(address))
                                    address = "San Juan St. Barangay 5";

                                // Check for duplicate
                                var existingResident = await _context.Residents
                                    .FirstOrDefaultAsync(r =>
                                        r.FirstName.ToLower() == firstName.ToLower() &&
                                        r.LastName.ToLower() == lastName.ToLower() &&
                                        r.DateOfBirth.Date == dateOfBirth.Date);

                                if (existingResident != null)
                                {
                                    if (!existingResident.BHWProfileId.HasValue && bhwProfileId.HasValue)
                                    {
                                        existingResident.BHWProfileId = bhwProfileId.Value;
                                        existingResident.UpdatedAt = DateTime.UtcNow;
                                        await _context.SaveChangesAsync();
                                        importedCount++;
                                    }
                                    continue;
                                }

                                // Create new resident
                                var resident = new Resident
                                {
                                    FirstName = firstName,
                                    LastName = lastName,
                                    MiddleName = columnMap.ContainsKey("MiddleName") ? worksheet.Cell(row, columnMap["MiddleName"]).GetString().Trim() : null,
                                    Suffix = columnMap.ContainsKey("Suffix") ? worksheet.Cell(row, columnMap["Suffix"]).GetString().Trim() : null,
                                    DateOfBirth = dateOfBirth,
                                    Gender = gender,
                                    Address = address,
                                    ContactNumber = columnMap.ContainsKey("ContactNumber") ? worksheet.Cell(row, columnMap["ContactNumber"]).GetString().Trim() : null,
                                    Email = columnMap.ContainsKey("Email") ? worksheet.Cell(row, columnMap["Email"]).GetString().Trim() : null,
                                    CivilStatus = columnMap.ContainsKey("CivilStatus") ? worksheet.Cell(row, columnMap["CivilStatus"]).GetString().Trim() : null,
                                    Occupation = columnMap.ContainsKey("Occupation") ? worksheet.Cell(row, columnMap["Occupation"]).GetString().Trim() : null,
                                    EmploymentStatus = columnMap.ContainsKey("EmploymentStatus") ? worksheet.Cell(row, columnMap["EmploymentStatus"]).GetString().Trim() : null,
                                    IsVoter = columnMap.ContainsKey("IsVoter") ?
                                        worksheet.Cell(row, columnMap["IsVoter"]).GetString().Trim().Equals("Yes", StringComparison.OrdinalIgnoreCase) : false,
                                    VoterId = columnMap.ContainsKey("VoterId") ? worksheet.Cell(row, columnMap["VoterId"]).GetString().Trim() : null,
                                    RelationshipToHead = columnMap.ContainsKey("RelationshipToHead") ? worksheet.Cell(row, columnMap["RelationshipToHead"]).GetString().Trim() : null,
                                    EducationalAttainment = columnMap.ContainsKey("EducationalAttainment") ? worksheet.Cell(row, columnMap["EducationalAttainment"]).GetString().Trim() : null,
                                    BloodType = columnMap.ContainsKey("BloodType") ? worksheet.Cell(row, columnMap["BloodType"]).GetString().Trim() : null,
                                    IsPWD = columnMap.ContainsKey("IsPWD") ?
                                        worksheet.Cell(row, columnMap["IsPWD"]).GetString().Trim().Equals("Yes", StringComparison.OrdinalIgnoreCase) : false,
                                    IsSenior = columnMap.ContainsKey("IsSenior") ?
                                        worksheet.Cell(row, columnMap["IsSenior"]).GetString().Trim().Equals("Yes", StringComparison.OrdinalIgnoreCase) : false,
                                    Notes = columnMap.ContainsKey("Notes") ? worksheet.Cell(row, columnMap["Notes"]).GetString().Trim() : null,
                                    BHWProfileId = bhwProfileId.Value,
                                    CreatedAt = DateTime.UtcNow
                                };

                                // Handle household lookup
                                if (columnMap.ContainsKey("Household"))
                                {
                                    var householdValue = worksheet.Cell(row, columnMap["Household"]).GetString().Trim();
                                    if (!string.IsNullOrWhiteSpace(householdValue))
                                    {
                                        var household = await _context.Households
                                            .FirstOrDefaultAsync(h => h.HouseholdNumber.ToLower() == householdValue.ToLower());
                                        if (household != null)
                                            resident.HouseholdId = household.Id;
                                    }
                                }

                                await _residentService.CreateResidentAsync(resident);
                                importedCount++;
                            }
                            catch (Exception ex)
                            {
                                errorCount++;
                                errors.Add($"Row {row}: {ex.Message}");
                            }
                        }

                        totalImported += importedCount;
                        totalErrors += errorCount;

                        results.Add(new
                        {
                            fileName = fileName,
                            filePath = filePath,
                            importedCount = importedCount,
                            errorCount = errorCount,
                            errors = errors.Take(20).ToList()
                        });
                    }
                    catch (Exception ex)
                    {
                        totalErrors++;
                        results.Add(new
                        {
                            fileName = fileName,
                            filePath = filePath,
                            importedCount = 0,
                            errorCount = 1,
                            errors = new List<string> { $"Error processing file: {ex.Message}" }
                        });
                    }
                }

                return Ok(new
                {
                    message = $"Batch import completed. Processed {excelFiles.Count} file(s). Imported: {totalImported}, Errors: {totalErrors}",
                    filesProcessed = excelFiles.Count,
                    totalImported = totalImported,
                    totalErrors = totalErrors,
                    results = results
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error importing residents from Downloads", error = ex.Message, stackTrace = ex.StackTrace });
            }
        }
    }

    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class HouseholdsController : ControllerBase
    {
        private readonly IResidentService _residentService;

        public HouseholdsController(IResidentService residentService)
        {
            _residentService = residentService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var households = await _residentService.GetAllHouseholdsAsync();
            return Ok(households);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var household = await _residentService.GetHouseholdByIdAsync(id);
            if (household == null) return NotFound();
            return Ok(household);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Household household)
        {
            var created = await _residentService.CreateHouseholdAsync(household);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
    }
}



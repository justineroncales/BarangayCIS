using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using BarangayCIS.API.Data;
using BarangayCIS.API.Models;

namespace BarangayCIS.API.Scripts
{
    public class ImportResidentsScript
    {
        public static async Task<int> ImportFromExcel(string filePath, string bhwName = "Emily rotairo")
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=BarangayCIS;Trusted_Connection=True;MultipleActiveResultSets=true");
            
            using var context = new ApplicationDbContext(optionsBuilder.Options);
            
            // Find BHW profile by name
            int? bhwProfileId = null;
            if (!string.IsNullOrWhiteSpace(bhwName))
            {
                var bhwNameParts = bhwName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (bhwNameParts.Length >= 2)
                {
                    var firstName = bhwNameParts[0];
                    var lastName = bhwNameParts[bhwNameParts.Length - 1];
                    var middleName = bhwNameParts.Length > 2 ? string.Join(" ", bhwNameParts.Skip(1).Take(bhwNameParts.Length - 2)) : null;

                    var bhwProfile = await context.BHWProfiles
                        .FirstOrDefaultAsync(b => 
                            b.FirstName.Equals(firstName, StringComparison.OrdinalIgnoreCase) &&
                            b.LastName.Equals(lastName, StringComparison.OrdinalIgnoreCase) &&
                            (middleName == null || (b.MiddleName != null && b.MiddleName.Equals(middleName, StringComparison.OrdinalIgnoreCase))));

                    if (bhwProfile != null)
                    {
                        bhwProfileId = bhwProfile.Id;
                        Console.WriteLine($"Found BHW Profile: {bhwProfile.FirstName} {bhwProfile.LastName} (ID: {bhwProfileId})");
                    }
                    else
                    {
                        // Try case-insensitive search with full name
                        var fullName = $"{firstName} {lastName}";
                        bhwProfile = await context.BHWProfiles
                            .FirstOrDefaultAsync(b => 
                                $"{b.FirstName} {b.LastName}".Equals(fullName, StringComparison.OrdinalIgnoreCase));
                        
                        if (bhwProfile != null)
                        {
                            bhwProfileId = bhwProfile.Id;
                            Console.WriteLine($"Found BHW Profile: {bhwProfile.FirstName} {bhwProfile.LastName} (ID: {bhwProfileId})");
                        }
                    }
                }
            }

            if (!bhwProfileId.HasValue)
            {
                Console.WriteLine($"ERROR: BHW profile '{bhwName}' not found. Please ensure the BHW is registered in the system.");
                return 1;
            }

            // Read Excel file
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"ERROR: File not found: {filePath}");
                return 1;
            }

            using var workbook = new XLWorkbook(filePath);
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

            Console.WriteLine($"Header row found at: {headerRow}");

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

            Console.WriteLine($"Found {columnMap.Count} columns mapped");

            // Validate required columns
            if (!columnMap.ContainsKey("FirstName") || !columnMap.ContainsKey("LastName") || 
                !columnMap.ContainsKey("DateOfBirth") || !columnMap.ContainsKey("Gender") || 
                !columnMap.ContainsKey("Address"))
            {
                Console.WriteLine("ERROR: Required columns not found. Please ensure the Excel file has: FirstName, LastName, DateOfBirth, Gender, and Address columns.");
                return 1;
            }

            // Process data rows
            var lastRow = worksheet.LastRowUsed().RowNumber();
            Console.WriteLine($"Processing rows {headerRow + 1} to {lastRow}...");

            for (int row = headerRow + 1; row <= lastRow; row++)
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
                    DateTime dateOfBirth;
                    var dobValue = worksheet.Cell(row, columnMap["DateOfBirth"]).GetString().Trim();
                    if (string.IsNullOrWhiteSpace(dobValue))
                    {
                        errors.Add($"Row {row}: DateOfBirth is required");
                        errorCount++;
                        continue;
                    }

                    if (!DateTime.TryParse(dobValue, out dateOfBirth))
                    {
                        // Try parsing as Excel date number
                        if (double.TryParse(dobValue, out var excelDate))
                        {
                            dateOfBirth = DateTime.FromOADate(excelDate);
                        }
                        else
                        {
                            errors.Add($"Row {row}: Invalid DateOfBirth format: {dobValue}");
                            errorCount++;
                            continue;
                        }
                    }

                    // Get gender
                    var gender = worksheet.Cell(row, columnMap["Gender"]).GetString().Trim();
                    if (string.IsNullOrWhiteSpace(gender))
                    {
                        errors.Add($"Row {row}: Gender is required");
                        errorCount++;
                        continue;
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

                    // Get address
                    var address = worksheet.Cell(row, columnMap["Address"]).GetString().Trim();
                    if (string.IsNullOrWhiteSpace(address))
                    {
                        errors.Add($"Row {row}: Address is required");
                        errorCount++;
                        continue;
                    }

                    // Create resident
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
                            var household = await context.Households
                                .FirstOrDefaultAsync(h => h.HouseholdNumber.Equals(householdValue, StringComparison.OrdinalIgnoreCase));
                            
                            if (household != null)
                            {
                                resident.HouseholdId = household.Id;
                            }
                        }
                    }

                    context.Residents.Add(resident);
                    await context.SaveChangesAsync();
                    importedCount++;
                    
                    if (importedCount % 10 == 0)
                    {
                        Console.WriteLine($"Imported {importedCount} residents...");
                    }
                }
                catch (Exception ex)
                {
                    errorCount++;
                    errors.Add($"Row {row}: {ex.Message}");
                    Console.WriteLine($"Error on row {row}: {ex.Message}");
                }
            }

            Console.WriteLine($"\nImport completed!");
            Console.WriteLine($"Imported: {importedCount}");
            Console.WriteLine($"Errors: {errorCount}");
            
            if (errors.Any())
            {
                Console.WriteLine("\nFirst 20 errors:");
                foreach (var error in errors.Take(20))
                {
                    Console.WriteLine($"  - {error}");
                }
            }

            return 0;
        }
    }
}



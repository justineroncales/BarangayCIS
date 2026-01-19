# Direct import script that connects to the database
# Usage: .\ImportResidentsDirect.ps1 -FilePath "C:\Users\Justine\Downloads\Resident Template BLOCK 5.xlsx"

param(
    [Parameter(Mandatory=$true)]
    [string]$FilePath,
    
    [Parameter(Mandatory=$false)]
    [string]$BhwName = "Emily rotairo",
    
    [Parameter(Mandatory=$false)]
    [string]$ConnectionString = "Server=(localdb)\mssqllocaldb;Database=BarangayCIS;Trusted_Connection=True;MultipleActiveResultSets=true"
)

$ErrorActionPreference = "Stop"

Write-Host "Starting resident import from: $FilePath" -ForegroundColor Green
Write-Host "Assigning BHW: $BhwName" -ForegroundColor Green

# Check if file exists
if (-not (Test-Path $FilePath)) {
    Write-Host "ERROR: File not found: $FilePath" -ForegroundColor Red
    exit 1
}

# Load required assemblies
Add-Type -Path ".\BarangayCIS.API\bin\Debug\net8.0\ClosedXML.dll" -ErrorAction SilentlyContinue
Add-Type -Path ".\BarangayCIS.API\bin\Debug\net8.0\Microsoft.EntityFrameworkCore.dll" -ErrorAction SilentlyContinue
Add-Type -Path ".\BarangayCIS.API\bin\Debug\net8.0\Microsoft.EntityFrameworkCore.SqlServer.dll" -ErrorAction SilentlyContinue

# Build the project first
Write-Host "Building project..." -ForegroundColor Yellow
$projectPath = Join-Path $PSScriptRoot "BarangayCIS.API\BarangayCIS.API.csproj"
Push-Location $PSScriptRoot
dotnet build $projectPath
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Build failed" -ForegroundColor Red
    Pop-Location
    exit 1
}
Pop-Location

# Create a C# program to do the import
$csharpCode = @"
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using BarangayCIS.API.Data;
using BarangayCIS.API.Models;

class Program
{
    static async Task Main(string[] args)
    {
        var filePath = @"$($FilePath.Replace('\', '\\').Replace('"', '\"'))";
        var bhwName = @"$($BhwName.Replace('"', '\"'))";
        var connectionString = @"$($ConnectionString.Replace('\', '\\').Replace('"', '\"'))";
        
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer(connectionString);
        
        using var context = new ApplicationDbContext(optionsBuilder.Options);
        
        // Find BHW profile
        int? bhwProfileId = null;
        if (!string.IsNullOrWhiteSpace(bhwName))
        {
            var bhwNameParts = bhwName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (bhwNameParts.Length >= 2)
            {
                var firstName = bhwNameParts[0];
                var lastName = bhwNameParts[bhwNameParts.Length - 1];
                
                var bhwProfile = await context.BHWProfiles
                    .FirstOrDefaultAsync(b => 
                        b.FirstName.Equals(firstName, StringComparison.OrdinalIgnoreCase) &&
                        b.LastName.Equals(lastName, StringComparison.OrdinalIgnoreCase));
                
                if (bhwProfile != null)
                {
                    bhwProfileId = bhwProfile.Id;
                    Console.WriteLine($\"Found BHW: {bhwProfile.FirstName} {bhwProfile.LastName} (ID: {bhwProfileId})\");
                }
            }
        }
        
        if (!bhwProfileId.HasValue)
        {
            Console.WriteLine($\"ERROR: BHW '{bhwName}' not found\");
            return;
        }
        
        // Read Excel
        using var workbook = new XLWorkbook(filePath);
        var worksheet = workbook.Worksheets.First();
        
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
        
        Console.WriteLine($\"Header row: {headerRow}\");
        
        // Map columns
        var columnMap = new Dictionary<string, int>();
        for (int col = 1; col <= worksheet.LastColumnUsed().ColumnNumber(); col++)
        {
            var header = worksheet.Cell(headerRow, col).GetString().Trim().ToLower();
            if (header.Contains("firstname") || header.Contains("first name")) columnMap["FirstName"] = col;
            else if (header.Contains("lastname") || header.Contains("last name")) columnMap["LastName"] = col;
            else if (header.Contains("middlename") || header.Contains("middle name")) columnMap["MiddleName"] = col;
            else if (header.Contains("suffix")) columnMap["Suffix"] = col;
            else if (header.Contains("dateofbirth") || header.Contains("date of birth") || header.Contains("birthdate")) columnMap["DateOfBirth"] = col;
            else if (header.Contains("gender")) columnMap["Gender"] = col;
            else if (header.Contains("address")) columnMap["Address"] = col;
            else if (header.Contains("contact") || header.Contains("phone")) columnMap["ContactNumber"] = col;
            else if (header.Contains("email")) columnMap["Email"] = col;
            else if (header.Contains("civil")) columnMap["CivilStatus"] = col;
            else if (header.Contains("occupation")) columnMap["Occupation"] = col;
            else if (header.Contains("employment")) columnMap["EmploymentStatus"] = col;
            else if (header.Contains("voter") && !header.Contains("id")) columnMap["IsVoter"] = col;
            else if (header.Contains("voterid")) columnMap["VoterId"] = col;
            else if (header.Contains("household")) columnMap["Household"] = col;
            else if (header.Contains("relationship")) columnMap["RelationshipToHead"] = col;
            else if (header.Contains("educational")) columnMap["EducationalAttainment"] = col;
            else if (header.Contains("blood")) columnMap["BloodType"] = col;
            else if (header.Contains("pwd")) columnMap["IsPWD"] = col;
            else if (header.Contains("senior")) columnMap["IsSenior"] = col;
            else if (header.Contains("notes")) columnMap["Notes"] = col;
        }
        
        Console.WriteLine($\"Mapped {columnMap.Count} columns\");
        
        if (!columnMap.ContainsKey("FirstName") || !columnMap.ContainsKey("LastName"))
        {
            Console.WriteLine("ERROR: Required columns not found");
            return;
        }
        
        int imported = 0;
        int errors = 0;
        var errorList = new List<string>();
        
        for (int row = headerRow + 1; row <= worksheet.LastRowUsed().RowNumber(); row++)
        {
            try
            {
                var firstName = worksheet.Cell(row, columnMap["FirstName"]).GetString().Trim();
                var lastName = worksheet.Cell(row, columnMap["LastName"]).GetString().Trim();
                
                if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
                    continue;
                
                var dobStr = worksheet.Cell(row, columnMap["DateOfBirth"]).GetString().Trim();
                if (!DateTime.TryParse(dobStr, out var dob))
                {
                    if (double.TryParse(dobStr, out var excelDate))
                        dob = DateTime.FromOADate(excelDate);
                    else
                    {
                        errors++;
                        errorList.Add($\"Row {row}: Invalid date format\");
                        continue;
                    }
                }
                
                var gender = worksheet.Cell(row, columnMap["Gender"]).GetString().Trim();
                if (gender.Equals("M", StringComparison.OrdinalIgnoreCase)) gender = "Male";
                else if (gender.Equals("F", StringComparison.OrdinalIgnoreCase)) gender = "Female";
                
                var address = worksheet.Cell(row, columnMap["Address"]).GetString().Trim();
                if (string.IsNullOrWhiteSpace(address))
                {
                    errors++;
                    errorList.Add($\"Row {row}: Address is required\");
                    continue;
                }
                
                var resident = new Resident
                {
                    FirstName = firstName,
                    LastName = lastName,
                    MiddleName = columnMap.ContainsKey("MiddleName") ? worksheet.Cell(row, columnMap["MiddleName"]).GetString().Trim() : null,
                    Suffix = columnMap.ContainsKey("Suffix") ? worksheet.Cell(row, columnMap["Suffix"]).GetString().Trim() : null,
                    DateOfBirth = dob,
                    Gender = gender,
                    Address = address,
                    ContactNumber = columnMap.ContainsKey("ContactNumber") ? worksheet.Cell(row, columnMap["ContactNumber"]).GetString().Trim() : null,
                    Email = columnMap.ContainsKey("Email") ? worksheet.Cell(row, columnMap["Email"]).GetString().Trim() : null,
                    CivilStatus = columnMap.ContainsKey("CivilStatus") ? worksheet.Cell(row, columnMap["CivilStatus"]).GetString().Trim() : null,
                    Occupation = columnMap.ContainsKey("Occupation") ? worksheet.Cell(row, columnMap["Occupation"]).GetString().Trim() : null,
                    EmploymentStatus = columnMap.ContainsKey("EmploymentStatus") ? worksheet.Cell(row, columnMap["EmploymentStatus"]).GetString().Trim() : null,
                    IsVoter = columnMap.ContainsKey("IsVoter") ? worksheet.Cell(row, columnMap["IsVoter"]).GetString().Trim().Equals("Yes", StringComparison.OrdinalIgnoreCase) : false,
                    VoterId = columnMap.ContainsKey("VoterId") ? worksheet.Cell(row, columnMap["VoterId"]).GetString().Trim() : null,
                    RelationshipToHead = columnMap.ContainsKey("RelationshipToHead") ? worksheet.Cell(row, columnMap["RelationshipToHead"]).GetString().Trim() : null,
                    EducationalAttainment = columnMap.ContainsKey("EducationalAttainment") ? worksheet.Cell(row, columnMap["EducationalAttainment"]).GetString().Trim() : null,
                    BloodType = columnMap.ContainsKey("BloodType") ? worksheet.Cell(row, columnMap["BloodType"]).GetString().Trim() : null,
                    IsPWD = columnMap.ContainsKey("IsPWD") ? worksheet.Cell(row, columnMap["IsPWD"]).GetString().Trim().Equals("Yes", StringComparison.OrdinalIgnoreCase) : false,
                    IsSenior = columnMap.ContainsKey("IsSenior") ? worksheet.Cell(row, columnMap["IsSenior"]).GetString().Trim().Equals("Yes", StringComparison.OrdinalIgnoreCase) : false,
                    Notes = columnMap.ContainsKey("Notes") ? worksheet.Cell(row, columnMap["Notes"]).GetString().Trim() : null,
                    BHWProfileId = bhwProfileId.Value,
                    CreatedAt = DateTime.UtcNow
                };
                
                if (columnMap.ContainsKey("Household"))
                {
                    var householdValue = worksheet.Cell(row, columnMap["Household"]).GetString().Trim();
                    if (!string.IsNullOrWhiteSpace(householdValue))
                    {
                        var household = await context.Households
                            .FirstOrDefaultAsync(h => h.HouseholdNumber.Equals(householdValue, StringComparison.OrdinalIgnoreCase));
                        if (household != null)
                            resident.HouseholdId = household.Id;
                    }
                }
                
                context.Residents.Add(resident);
                await context.SaveChangesAsync();
                imported++;
                
                if (imported % 10 == 0)
                    Console.WriteLine($\"Imported {imported} residents...\");
            }
            catch (Exception ex)
            {
                errors++;
                errorList.Add($\"Row {row}: {ex.Message}\");
                Console.WriteLine($\"Error on row {row}: {ex.Message}\");
            }
        }
        
        Console.WriteLine($\"\nImport completed! Imported: {imported}, Errors: {errors}\");
        if (errorList.Any())
        {
            Console.WriteLine(\"\nFirst 20 errors:\");
            foreach (var err in errorList.Take(20))
                Console.WriteLine($\"  - {err}\");
        }
    }
}
"@

# Save and compile the C# code
$tempDir = Join-Path $env:TEMP "ImportResidents_$(Get-Date -Format 'yyyyMMddHHmmss')"
New-Item -ItemType Directory -Path $tempDir -Force | Out-Null
$csFile = Join-Path $tempDir "Program.cs"
$csharpCode | Out-File -FilePath $csFile -Encoding UTF8

# Copy required DLLs
$binPath = Join-Path $PSScriptRoot "BarangayCIS.API\bin\Debug\net8.0"
Copy-Item "$binPath\*.dll" -Destination $tempDir -ErrorAction SilentlyContinue
Copy-Item "$binPath\*.pdb" -Destination $tempDir -ErrorAction SilentlyContinue

# Create a simple project file
$csprojContent = @"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <Reference Include="ClosedXML">
      <HintPath>$binPath\ClosedXML.dll</HintPath>
    </Reference>
    <Reference Include="Microsoft.EntityFrameworkCore">
      <HintPath>$binPath\Microsoft.EntityFrameworkCore.dll</HintPath>
    </Reference>
    <Reference Include="Microsoft.EntityFrameworkCore.SqlServer">
      <HintPath>$binPath\Microsoft.EntityFrameworkCore.SqlServer.dll</HintPath>
    </Reference>
  </ItemGroup>
</Project>
"@

Write-Host "This approach is complex. Let me try a simpler method using the API endpoint..." -ForegroundColor Yellow

# Actually, let's just use curl/Invoke-WebRequest to call the API
Write-Host "Please ensure the API server is running, then we can call the import endpoint." -ForegroundColor Yellow
Write-Host "Or use: dotnet run --project BarangayCIS.API" -ForegroundColor Yellow



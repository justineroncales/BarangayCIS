using ClosedXML.Excel;

namespace BarangayCIS.API.Utilities
{
    public static class ResidentTemplateGenerator
    {
        public static void GenerateTemplateFile(string outputPath)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Residents");

            // Add instructions at the top
            worksheet.Cell(1, 1).Value = "RESIDENT DATA TEMPLATE - INSTRUCTIONS";
            worksheet.Cell(1, 1).Style.Font.Bold = true;
            worksheet.Cell(1, 1).Style.Font.FontSize = 14;
            worksheet.Cell(2, 1).Value = "Fields marked with * are required. Delete example rows (rows 6-7) before entering your data.";
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

            // Save the file
            workbook.SaveAs(outputPath);
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using BarangayCIS.API.Utilities;
using System.IO;

namespace BarangayCIS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GenerateTemplateController : ControllerBase
    {
        [HttpGet("resident-template")]
        public IActionResult DownloadResidentTemplate()
        {
            string fileName = "ResidentTemplate.xlsx";
            string filePath = Path.Combine(Path.GetTempPath(), fileName);

            // Generate the Excel file
            ResidentTemplateGenerator.GenerateTemplateFile(filePath);

            // Read file into memory
            var bytes = System.IO.File.ReadAllBytes(filePath);

            // Return as downloadable file
            return File(
                bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName
            );
        }
    }
}

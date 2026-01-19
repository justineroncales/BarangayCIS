using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BarangayCIS.API.Services;
using BarangayCIS.API.Models;
using BarangayCIS.API.Data;
using System.Text;

namespace BarangayCIS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CertificatesController : ControllerBase
    {
        private readonly ICertificateService _certificateService;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public CertificatesController(ICertificateService certificateService, ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _certificateService = certificateService;
            _context = context;
            _environment = environment;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? type, [FromQuery] string? status)
        {
            var certificates = await _certificateService.GetAllCertificatesAsync(type, status);
            return Ok(certificates);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var certificate = await _certificateService.GetCertificateByIdAsync(id);
            if (certificate == null) return NotFound();
            return Ok(certificate);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCertificateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Convert DTO to Entity
                var certificate = new Certificate
                {
                    CertificateType = dto.CertificateType,
                    ResidentId = dto.ResidentId,
                    Purpose = dto.Purpose,
                    IssueDate = dto.IssueDate,
                    ExpiryDate = dto.ExpiryDate,
                    Status = dto.Status,
                    IssuedBy = dto.IssuedBy
                };

                var created = await _certificateService.CreateCertificateAsync(certificate);
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
                    message = "Database error while creating certificate", 
                    error = errorMessage,
                    details = ex.InnerException?.ToString()
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    message = "An error occurred while creating the certificate", 
                    error = ex.Message,
                    details = ex.ToString()
                });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCertificateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Get existing certificate
            var existing = await _certificateService.GetCertificateByIdAsync(id);
            if (existing == null) return NotFound();

            // Update only provided fields
            if (dto.CertificateType != null) existing.CertificateType = dto.CertificateType;
            if (dto.ResidentId.HasValue) existing.ResidentId = dto.ResidentId.Value;
            if (dto.Purpose != null) existing.Purpose = dto.Purpose;
            if (dto.IssueDate.HasValue) existing.IssueDate = dto.IssueDate.Value;
            if (dto.ExpiryDate.HasValue) existing.ExpiryDate = dto.ExpiryDate.Value;
            if (dto.Status != null) existing.Status = dto.Status;
            if (dto.IssuedBy != null) existing.IssuedBy = dto.IssuedBy;
            if (dto.PickedUpAt.HasValue) existing.PickedUpAt = dto.PickedUpAt;
            if (dto.SMSNotificationSent != null) existing.SMSNotificationSent = dto.SMSNotificationSent;

            var updated = await _certificateService.UpdateCertificateAsync(id, existing);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _certificateService.DeleteCertificateAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }

        [HttpPost("{id}/generate-qr")]
        public async Task<IActionResult> GenerateQRCode(int id)
        {
            var qrCode = await _certificateService.GenerateQRCodeAsync(id);
            return Ok(new { qrCodeImage = qrCode });
        }

        [HttpGet("{id}/print")]
        [AllowAnonymous]
        public async Task<IActionResult> Print(int id)
        {
            var certificate = await _context.Certificates
                .Include(c => c.Resident)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (certificate == null)
            {
                return NotFound();
            }

            var html = GenerateCertificateHtml(certificate);
            return Content(html, "text/html", Encoding.UTF8);
        }

        private string GenerateCertificateHtml(Certificate cert)
        {
            var resident = cert.Resident;
            var logosPath = Path.Combine(_environment.ContentRootPath, "Assets", "Logos");
            var templatePath = Path.Combine(_environment.ContentRootPath, "Templates", "CertificateTemplate.html");
            
            // Load HTML template
            string htmlTemplate;
            if (System.IO.File.Exists(templatePath))
            {
                htmlTemplate = System.IO.File.ReadAllText(templatePath);
            }
            else
            {
                // Fallback to inline generation if template not found
                return GenerateCertificateHtmlInline(cert);
            }
            
            // Try to load logos as base64
            string municipalLogoImg = "";
            string barangayLogoImg = "";
            
            var municipalLogoPath = Path.Combine(logosPath, "municipal-logo.png");
            var barangayLogoPath = Path.Combine(logosPath, "barangay-logo.png");
            
            if (System.IO.File.Exists(municipalLogoPath))
            {
                var bytes = System.IO.File.ReadAllBytes(municipalLogoPath);
                var base64 = Convert.ToBase64String(bytes);
                municipalLogoImg = $"<img src='data:image/png;base64,{base64}' class='logo' alt='Municipal Logo' />";
            }
            
            if (System.IO.File.Exists(barangayLogoPath))
            {
                var bytes = System.IO.File.ReadAllBytes(barangayLogoPath);
                var base64 = Convert.ToBase64String(bytes);
                barangayLogoImg = $"<img src='data:image/png;base64,{base64}' class='logo' alt='Barangay Logo' />";
            }

            // Format resident data
            var residentName = $"{resident?.FirstName?.ToUpper()} {resident?.MiddleName?.ToUpper()} {resident?.LastName?.ToUpper()}".Trim();
            var civilStatus = !string.IsNullOrEmpty(resident?.CivilStatus) ? $"<strong>{resident.CivilStatus}</strong>, " : "";
            var birthDate = (resident?.DateOfBirth ?? DateTime.Now).ToString("MMMM dd, yyyy").ToUpper();
            var residentAddress = resident?.Address ?? "N/A";
            
            var issueDate = cert.IssueDate;
            var day = issueDate.Day;
            var suffix = (day % 10 == 1 && day != 11) ? "th" : 
                        (day % 10 == 2 && day != 12) ? "nd" : 
                        (day % 10 == 3 && day != 13) ? "rd" : "th";
            var issueDateText = $"{day}{suffix} day of {issueDate:MMMM yyyy}";
            
            // Replace placeholders
            htmlTemplate = htmlTemplate.Replace("{{MUNICIPAL_LOGO}}", municipalLogoImg);
            htmlTemplate = htmlTemplate.Replace("{{BARANGAY_LOGO}}", barangayLogoImg);
            htmlTemplate = htmlTemplate.Replace("{{RESIDENT_NAME}}", residentName);
            htmlTemplate = htmlTemplate.Replace("{{CIVIL_STATUS}}", civilStatus);
            htmlTemplate = htmlTemplate.Replace("{{BIRTH_DATE}}", birthDate);
            htmlTemplate = htmlTemplate.Replace("{{RESIDENT_ADDRESS}}", residentAddress);
            htmlTemplate = htmlTemplate.Replace("{{ISSUE_DATE}}", issueDateText);
            htmlTemplate = htmlTemplate.Replace("{{CERTIFICATE_NUMBER}}", cert.CertificateNumber);
            htmlTemplate = htmlTemplate.Replace("{{ISSUE_DATE_FORMATTED}}", cert.IssueDate.ToString("MM-dd-yyyy"));
            htmlTemplate = htmlTemplate.Replace("{{QR_CODE}}", "");

            return htmlTemplate;
        }

        private string GenerateCertificateHtmlInline(Certificate cert)
        {
            var resident = cert.Resident;
            var logosPath = Path.Combine(_environment.ContentRootPath, "Assets", "Logos");
            
            // Try to load logos as base64
            string municipalLogo = "";
            string barangayLogo = "";
            
            var municipalLogoPath = Path.Combine(logosPath, "municipal-logo.png");
            var barangayLogoPath = Path.Combine(logosPath, "barangay-logo.png");
            
            if (System.IO.File.Exists(municipalLogoPath))
            {
                var bytes = System.IO.File.ReadAllBytes(municipalLogoPath);
                municipalLogo = Convert.ToBase64String(bytes);
            }
            
            if (System.IO.File.Exists(barangayLogoPath))
            {
                var bytes = System.IO.File.ReadAllBytes(barangayLogoPath);
                barangayLogo = Convert.ToBase64String(bytes);
            }

            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html>");
            sb.AppendLine("<head>");
            sb.AppendLine("<meta charset='UTF-8'>");
            sb.AppendLine("<title>BARANGAY CLEARANCE</title>");
            sb.AppendLine("<style>");
            sb.AppendLine("@page { margin: 0.5in; size: legal; }");
            sb.AppendLine("@media print { @page { size: 8.5in 14in; margin: 0.5in; } body { width: 100% !important; max-width: 100% !important; margin: 0 !important; padding: 0.5in !important; box-shadow: none !important; } }");
            sb.AppendLine("* { margin: 0; padding: 0; box-sizing: border-box; }");
            sb.AppendLine("html { background: #f0f0f0; }");
            sb.AppendLine("body { font-family: 'Times New Roman', serif; font-size: 11pt; margin: 20px auto; padding: 20px; position: relative; min-height: 13.5in; width: 8.5in; max-width: 8.5in; background: white; box-shadow: 0 0 10px rgba(0,0,0,0.1); }");
            
            // Header styles - matching original document exactly
            sb.AppendLine(".header { display: flex; align-items: flex-start; justify-content: space-between; margin-bottom: 10px; padding-bottom: 10px; border-bottom: 1px solid #000; }");
            sb.AppendLine(".header-logos { display: flex; gap: 12px; align-items: flex-start; }");
            sb.AppendLine(".logo { width: 100px; height: 100px; object-fit: contain; display: block; }");
            sb.AppendLine(".header-text { flex: 1; text-align: center; padding: 0 15px; }");
            sb.AppendLine(".header-text h1 { margin: 0; font-size: 11pt; font-weight: bold; line-height: 1.2; }");
            sb.AppendLine(".header-text h2 { margin: 2px 0; font-size: 10pt; font-weight: bold; line-height: 1.2; }");
            sb.AppendLine(".header-text p { margin: 2px 0; font-size: 9pt; line-height: 1.2; }");
            sb.AppendLine(".header-pb { text-align: right; min-width: 150px; }");
            sb.AppendLine(".pb-photo { width: 70px; height: 85px; border: 1px solid #000; object-fit: cover; margin-bottom: 5px; display: block; margin-left: auto; }");
            sb.AppendLine(".pb-name { font-size: 9pt; font-weight: bold; margin-bottom: 2px; text-align: right; }");
            sb.AppendLine(".pb-title { font-size: 8.5pt; text-align: right; }");
            
            // Main two-column layout
            sb.AppendLine(".main-wrapper { display: flex; gap: 20px; margin-top: 15px; align-items: flex-start; }");
            sb.AppendLine(".left-panel { width: 40%; border-right: 1px solid #ddd; padding-right: 15px; }");
            sb.AppendLine(".right-panel { width: 60%; padding-left: 15px; }");
            
            // Left column - Council
            sb.AppendLine(".council-header { font-size: 12pt; font-weight: bold; text-align: center; margin-bottom: 10px; text-decoration: underline; }");
            sb.AppendLine(".council-list { font-size: 9pt; line-height: 1.6; }");
            sb.AppendLine(".council-item { margin-bottom: 6px; }");
            sb.AppendLine(".council-item .name { font-weight: bold; display: inline; }");
            sb.AppendLine(".council-item .role { font-size: 8.5pt; }");
            
            // Right column - Certificate
            sb.AppendLine(".cert-header { font-size: 14pt; font-weight: bold; text-align: center; margin: 10px 0 12px 0; text-decoration: underline; }");
            sb.AppendLine(".cert-body { font-size: 10.5pt; line-height: 1.8; text-align: justify; }");
            sb.AppendLine(".cert-body p { margin-bottom: 8px; }");
            
            // Signatures
            sb.AppendLine(".signatures-wrapper { margin-top: 50px; display: flex; justify-content: space-between; gap: 20px; }");
            sb.AppendLine(".sig-left { width: 48%; text-align: center; }");
            sb.AppendLine(".sig-right { width: 48%; text-align: center; }");
            sb.AppendLine(".sig-line { border-top: 1px solid #000; margin-top: 55px; padding-top: 5px; }");
            sb.AppendLine(".sig-name { font-weight: bold; font-size: 10pt; }");
            sb.AppendLine(".sig-title { font-size: 9pt; margin-top: 3px; }");
            sb.AppendLine(".sig-prefix { font-size: 9pt; margin-bottom: 5px; }");
            
            // Footer elements
            sb.AppendLine(".dry-seal { position: absolute; right: 40px; top: 50%; transform: translateY(-50%); opacity: 0.2; font-size: 8pt; text-align: center; color: #999; border: 1px dashed #ccc; padding: 10px; border-radius: 50px; }");
            sb.AppendLine(".cert-info { position: absolute; bottom: 90px; right: 30px; font-size: 9pt; }");
            sb.AppendLine(".cert-info p { margin: 2px 0; }");
            sb.AppendLine(".footer-text { position: absolute; bottom: 25px; left: 50%; transform: translateX(-50%); font-size: 9pt; font-style: italic; text-align: center; width: 100%; }");
            sb.AppendLine(".qr-placeholder { position: absolute; bottom: 90px; left: 30px; }");
            sb.AppendLine(".qr-placeholder img { width: 75px; height: 75px; }");
            sb.AppendLine("</style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");

            // Header Section
            sb.AppendLine("<div class='header'>");
            sb.AppendLine("<div class='header-logos'>");
            if (!string.IsNullOrEmpty(municipalLogo))
            {
                sb.AppendLine($"<img src='data:image/png;base64,{municipalLogo}' class='logo' alt='Municipal Logo' />");
            }
            if (!string.IsNullOrEmpty(barangayLogo))
            {
                sb.AppendLine($"<img src='data:image/png;base64,{barangayLogo}' class='logo' alt='Barangay Logo' />");
            }
            sb.AppendLine("</div>");
            sb.AppendLine("<div class='header-text'>");
            sb.AppendLine("<h1>REPUBLIC OF THE PHILIPPINES</h1>");
            sb.AppendLine("<h2>PROVINCE OF BATANGAS</h2>");
            sb.AppendLine("<h2>MUNICIPALITY OF LIAN</h2>");
            sb.AppendLine("<h2>BARANGAY 5</h2>");
            sb.AppendLine("<p>BarangaysingkoJBV@gmail.com</p>");
            sb.AppendLine("<p>San Juan St. Barangay 5, Lian Batangas</p>");
            sb.AppendLine("</div>");
            sb.AppendLine("<div class='header-pb'>");
            // Photo placeholder - you can add photo later if needed
            sb.AppendLine("<div class='pb-name'>JASON B. VERGARA</div>");
            sb.AppendLine("<div class='pb-title'>Punong Barangay</div>");
            sb.AppendLine("</div>");
            sb.AppendLine("</div>");

            // Main Content - Two Columns
            sb.AppendLine("<div class='main-wrapper'>");
            
            // Left Column - Council List
            sb.AppendLine("<div class='left-panel'>");
            sb.AppendLine("<div class='council-header'>BARANGAY COUNCIL</div>");
            sb.AppendLine("<div class='council-list'>");
            sb.AppendLine("<div class='council-item'><span class='name'>PERFECTO R. DELAS ALAS:</span> <span class='role'>Chairman, Committee on Budget Appropriation and Barangay Affairs</span></div>");
            sb.AppendLine("<div class='council-item'><span class='name'>MARK JOSEPH R. ROTAIRO:</span> <span class='role'>Chairman, Committee on Human Rights and Public Works and Infrastructure</span></div>");
            sb.AppendLine("<div class='council-item'><span class='name'>RYAN CARLO Y. RIVERA:</span> <span class='role'>Chairman, Committee on Sports Culture and Physical Fitness</span></div>");
            sb.AppendLine("<div class='council-item'><span class='name'>CLEMENCIA C. DAVID:</span> <span class='role'>Chairman, Committee on Education and Social Services</span></div>");
            sb.AppendLine("<div class='council-item'><span class='name'>LEODEGARIO L. CABALI:</span> <span class='role'>Chairman, Committee on Peace and Order and Public Safety</span></div>");
            sb.AppendLine("<div class='council-item'><span class='name'>PRECILA B. CABALI:</span> <span class='role'>Chairman, Committee on Health and Anti-Illegal Drugs, Housing and Community Development</span></div>");
            sb.AppendLine("<div class='council-item'><span class='name'>EDGARDO J. BAGUNAS:</span> <span class='role'>Chairman, Committee on Ways and Means, Clean and Green, Environmental Protection and Beautification</span></div>");
            sb.AppendLine("<div class='council-item'><span class='name'>SK chairman:</span> <span class='role'>Chairman Committee on Youth and Sports Development</span></div>");
            sb.AppendLine("<div class='council-item'><span class='name'>ROSE ANN D. CORPUZ:</span> <span class='role'>Secretary</span></div>");
            sb.AppendLine("<div class='council-item'><span class='name'>CORAZON B. GOMEZ:</span> <span class='role'>Treasurer</span></div>");
            sb.AppendLine("</div>");
            sb.AppendLine("</div>");
            
            // Right Column - Certificate
            sb.AppendLine("<div class='right-panel'>");
            sb.AppendLine("<div class='cert-header'>BARANGAY CLEARANCE</div>");
            sb.AppendLine("<div class='cert-body'>");
            sb.AppendLine("<p>To whom it May Concern,</p>");
            sb.AppendLine("<p>");
            sb.AppendLine($"This is to certify that <strong>{resident?.FirstName?.ToUpper()} {resident?.MiddleName?.ToUpper()} {resident?.LastName?.ToUpper()}</strong>, ");
            if (!string.IsNullOrEmpty(resident?.CivilStatus))
            {
                sb.AppendLine($"<strong>{resident.CivilStatus}</strong>, ");
            }
            sb.AppendLine($"Born on <strong>{(resident?.DateOfBirth ?? DateTime.Now).ToString("MMMM dd, yyyy").ToUpper()}</strong> ");
            sb.AppendLine($"resident of <strong>{resident?.Address ?? "N/A"}</strong>,");
            sb.AppendLine("</p>");
            sb.AppendLine("<p>");
            sb.AppendLine("He/She is Known to be a person with good moral character and law abiding citizen for He/She was not commited nor been involved in any kind unlawful activities in this Barangay.");
            sb.AppendLine("</p>");
            
            var issueDate = cert.IssueDate;
            var day = issueDate.Day;
            var suffix = (day % 10 == 1 && day != 11) ? "th" : 
                        (day % 10 == 2 && day != 12) ? "nd" : 
                        (day % 10 == 3 && day != 13) ? "rd" : "th";
            
            sb.AppendLine($"<p>Signed this <strong>{day}{suffix} day of {issueDate:MMMM yyyy}</strong> at barangay 5, lian batangas.</p>");
            sb.AppendLine("</div>");
            
            // Signatures in right column
            sb.AppendLine("<div class='signatures-wrapper'>");
            sb.AppendLine("<div class='sig-left'>");
            sb.AppendLine("<div class='sig-line'>");
            sb.AppendLine("<div class='sig-name'>HON. JASON B. VERGARA</div>");
            sb.AppendLine("<div class='sig-title'>PUNONG BARANGAY</div>");
            sb.AppendLine("</div>");
            sb.AppendLine("</div>");
            sb.AppendLine("<div class='sig-right'>");
            sb.AppendLine("<div class='sig-prefix'><strong>For and by the authority of the</strong></div>");
            sb.AppendLine("<div class='sig-line'>");
            sb.AppendLine("<div class='sig-name'>ROSE ANN D. CORPUZ</div>");
            sb.AppendLine("<div class='sig-title'>Barangay Secretary</div>");
            sb.AppendLine("</div>");
            sb.AppendLine("</div>");
            sb.AppendLine("</div>");
            
            sb.AppendLine("</div>"); // End right panel
            sb.AppendLine("</div>"); // End main wrapper

            // Dry seal watermark
            sb.AppendLine("<div class='dry-seal'>NOT VALID WITHOUT<br>OFFICIAL DRY SEAL</div>");

            // Certificate details
            sb.AppendLine("<div class='cert-info'>");
            sb.AppendLine($"<p><strong>CLEARANCE NO:</strong> {cert.CertificateNumber}</p>");
            sb.AppendLine("<p><strong>OR NO. :</strong> </p>");
            sb.AppendLine($"<p><strong>ISSUED ON :</strong> {cert.IssueDate:MM-dd-yyyy}</p>");
            sb.AppendLine("<p><strong>ISSUED AT :</strong> BARANGAY 5, LIAN BATANGAS</p>");
            sb.AppendLine("</div>");

            // QR Code
            if (!string.IsNullOrEmpty(cert.QRCodeImagePath))
            {
                sb.AppendLine("<div class='qr-placeholder'>");
                sb.AppendLine($"<img src='data:image/png;base64,{cert.QRCodeImagePath}' alt='QR Code' />");
                sb.AppendLine("</div>");
            }

            // Footer motto
            sb.AppendLine("<div class='footer-text'>");
            sb.AppendLine("\"Malasakit at pagmamahal ang para sa inyo ay para sa inyo\"");
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
    }
}


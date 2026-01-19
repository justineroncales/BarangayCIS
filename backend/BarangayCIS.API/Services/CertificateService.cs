using Microsoft.EntityFrameworkCore;
using QRCoder;
using System.Text;
using BarangayCIS.API.Data;
using BarangayCIS.API.Models;

namespace BarangayCIS.API.Services
{
    public class CertificateService : ICertificateService
    {
        private readonly ApplicationDbContext _context;

        public CertificateService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Certificate>> GetAllCertificatesAsync(string? type = null, string? status = null)
        {
            var query = _context.Certificates.Include(c => c.Resident).AsQueryable();

            if (!string.IsNullOrWhiteSpace(type))
            {
                query = query.Where(c => c.CertificateType == type);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(c => c.Status == status);
            }

            return await query.OrderByDescending(c => c.CreatedAt).ToListAsync();
        }

        public async Task<Certificate?> GetCertificateByIdAsync(int id)
        {
            return await _context.Certificates
                .Include(c => c.Resident)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Certificate> CreateCertificateAsync(Certificate certificate)
        {
            // Validate resident exists
            var resident = await _context.Residents.FindAsync(certificate.ResidentId);
            if (resident == null)
            {
                throw new ArgumentException("Resident not found");
            }

            // Validate certificate type
            if (string.IsNullOrWhiteSpace(certificate.CertificateType))
            {
                throw new ArgumentException("Certificate type is required");
            }

            // Generate unique certificate number
            var year = DateTime.Now.Year;
            var prefix = certificate.CertificateType.Length >= 3 
                ? certificate.CertificateType.Substring(0, 3).ToUpper() 
                : certificate.CertificateType.ToUpper();
            
            // Get count of certificates for this type in current year
            var count = await _context.Certificates
                .CountAsync(c => c.CertificateType == certificate.CertificateType && 
                                c.CreatedAt.Year == year);
            
            // Generate certificate number and ensure uniqueness
            string certificateNumber;
            int attempts = 0;
            do
            {
                certificateNumber = $"{prefix}-{year}-{(count + attempts + 1):D5}";
                var exists = await _context.Certificates
                    .AnyAsync(c => c.CertificateNumber == certificateNumber);
                
                if (!exists) break;
                attempts++;
                
                if (attempts > 100) // Safety limit
                {
                    throw new InvalidOperationException("Unable to generate unique certificate number");
                }
            } while (true);
            
            certificate.CertificateNumber = certificateNumber;
            certificate.CreatedAt = DateTime.UtcNow;
            
            // Set default status if not provided
            if (string.IsNullOrWhiteSpace(certificate.Status))
            {
                certificate.Status = "Pending";
            }

            // Ensure IssueDate is set
            if (certificate.IssueDate == default)
            {
                certificate.IssueDate = DateTime.UtcNow;
            }

            try
            {
                _context.Certificates.Add(certificate);
                await _context.SaveChangesAsync();

                // Generate QR code after saving (to avoid transaction issues)
                try
                {
                    await GenerateQRCodeAsync(certificate.Id);
                }
                catch (Exception qrEx)
                {
                    // Log QR code generation error but don't fail the certificate creation
                    // The certificate is already created, QR code can be generated later
                    System.Diagnostics.Debug.WriteLine($"QR code generation failed: {qrEx.Message}");
                }

                return certificate;
            }
            catch (DbUpdateException ex)
            {
                // Log the inner exception for debugging
                var innerException = ex.InnerException?.Message ?? ex.Message;
                throw new InvalidOperationException($"Failed to create certificate: {innerException}", ex);
            }
        }

        public async Task<Certificate?> UpdateCertificateAsync(int id, Certificate certificate)
        {
            var existing = await _context.Certificates.FindAsync(id);
            if (existing == null) return null;

            existing.CertificateType = certificate.CertificateType;
            existing.Purpose = certificate.Purpose;
            existing.IssueDate = certificate.IssueDate;
            existing.ExpiryDate = certificate.ExpiryDate;
            existing.Status = certificate.Status;
            existing.IssuedBy = certificate.IssuedBy;
            existing.PickedUpAt = certificate.PickedUpAt;
            existing.SMSNotificationSent = certificate.SMSNotificationSent;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteCertificateAsync(int id)
        {
            var certificate = await _context.Certificates.FindAsync(id);
            if (certificate == null) return false;

            _context.Certificates.Remove(certificate);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<string> GenerateQRCodeAsync(int certificateId)
        {
            var certificate = await _context.Certificates
                .Include(c => c.Resident)
                .FirstOrDefaultAsync(c => c.Id == certificateId);

            if (certificate == null) return string.Empty;

            // Ensure Resident is loaded
            if (certificate.Resident == null && certificate.ResidentId > 0)
            {
                certificate.Resident = await _context.Residents.FindAsync(certificate.ResidentId);
            }

            var qrData = new StringBuilder();
            qrData.AppendLine($"Certificate Number: {certificate.CertificateNumber}");
            qrData.AppendLine($"Type: {certificate.CertificateType}");
            if (certificate.Resident != null)
            {
                qrData.AppendLine($"Resident: {certificate.Resident.FirstName} {certificate.Resident.LastName}");
            }
            qrData.AppendLine($"Issue Date: {certificate.IssueDate:yyyy-MM-dd}");
            qrData.AppendLine($"Expiry Date: {certificate.ExpiryDate:yyyy-MM-dd}");

            certificate.QRCodeData = qrData.ToString();

            // Generate QR code image (base64)
            using var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(certificate.QRCodeData, QRCodeGenerator.ECCLevel.Q);
            
            // Use PngByteQRCode for .NET Core compatibility
            using var pngByteQrCode = new PngByteQRCode(qrCodeData);
            var qrBytes = pngByteQrCode.GetGraphic(20);
            certificate.QRCodeImagePath = Convert.ToBase64String(qrBytes);

            await _context.SaveChangesAsync();
            return certificate.QRCodeImagePath;
        }
    }
}


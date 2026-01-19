using BarangayCIS.API.Models;

namespace BarangayCIS.API.Services
{
    public interface ICertificateService
    {
        Task<IEnumerable<Certificate>> GetAllCertificatesAsync(string? type = null, string? status = null);
        Task<Certificate?> GetCertificateByIdAsync(int id);
        Task<Certificate> CreateCertificateAsync(Certificate certificate);
        Task<Certificate?> UpdateCertificateAsync(int id, Certificate certificate);
        Task<bool> DeleteCertificateAsync(int id);
        Task<string> GenerateQRCodeAsync(int certificateId);
    }
}



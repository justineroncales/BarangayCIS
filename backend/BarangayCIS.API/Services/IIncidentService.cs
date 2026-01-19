using BarangayCIS.API.Models;

namespace BarangayCIS.API.Services
{
    public interface IIncidentService
    {
        Task<IEnumerable<Incident>> GetAllIncidentsAsync(string? type = null, string? status = null);
        Task<Incident?> GetIncidentByIdAsync(int id);
        Task<Incident> CreateIncidentAsync(Incident incident);
        Task<Incident?> UpdateIncidentAsync(int id, Incident incident);
        Task<bool> DeleteIncidentAsync(int id);
    }
}



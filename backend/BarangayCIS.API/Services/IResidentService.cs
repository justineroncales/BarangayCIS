using BarangayCIS.API.Models;

namespace BarangayCIS.API.Services
{
    public interface IResidentService
    {
        Task<IEnumerable<Resident>> GetAllResidentsAsync(string? search = null);
        Task<PagedResult<Resident>> GetResidentsPagedAsync(int pageNumber, int pageSize, string? search = null);
        Task<Resident?> GetResidentByIdAsync(int id);
        Task<Resident> CreateResidentAsync(Resident resident);
        Task<Resident?> UpdateResidentAsync(int id, Resident resident);
        Task<bool> DeleteResidentAsync(int id);
        Task<bool> ForceDeleteResidentAsync(int id);
        Task<IEnumerable<Household>> GetAllHouseholdsAsync();
        Task<Household?> GetHouseholdByIdAsync(int id);
        Task<Household> CreateHouseholdAsync(Household household);
        Task<IEnumerable<Resident>> GetResidentsByBHWIdAsync(int bhwProfileId, string? search = null);
        Task<PagedResult<Resident>> GetResidentsByBHWIdPagedAsync(int bhwProfileId, int pageNumber, int pageSize, string? search = null);
        Task<object> GetBHWResidentStatisticsAsync(int bhwProfileId);
    }
}



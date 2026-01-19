using Microsoft.EntityFrameworkCore;
using BarangayCIS.API.Data;
using BarangayCIS.API.Models;

namespace BarangayCIS.API.Services
{
    public class ResidentService : IResidentService
    {
        private readonly ApplicationDbContext _context;

        public ResidentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Resident>> GetAllResidentsAsync(string? search = null)
        {
            var query = _context.Residents
                .Include(r => r.Household)
                .Include(r => r.BHWProfile)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();
                query = query.Where(r =>
                    r.FirstName.ToLower().Contains(search) ||
                    r.LastName.ToLower().Contains(search) ||
                    r.MiddleName != null && r.MiddleName.ToLower().Contains(search) ||
                    r.Address.ToLower().Contains(search) ||
                    r.ContactNumber != null && r.ContactNumber.Contains(search));
            }

            return await query.OrderBy(r => r.LastName).ThenBy(r => r.FirstName).ToListAsync();
        }

        public async Task<PagedResult<Resident>> GetResidentsPagedAsync(int pageNumber, int pageSize, string? search = null)
        {
            var query = _context.Residents
                .Include(r => r.Household)
                .Include(r => r.BHWProfile)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();
                query = query.Where(r =>
                    r.FirstName.ToLower().Contains(search) ||
                    r.LastName.ToLower().Contains(search) ||
                    r.MiddleName != null && r.MiddleName.ToLower().Contains(search) ||
                    r.Address.ToLower().Contains(search) ||
                    r.ContactNumber != null && r.ContactNumber.Contains(search));
            }

            var totalCount = await query.CountAsync();

            var residents = await query
                .OrderBy(r => r.LastName)
                .ThenBy(r => r.FirstName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Resident>
            {
                Data = residents,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        public async Task<Resident?> GetResidentByIdAsync(int id)
        {
            return await _context.Residents
                .Include(r => r.Household)
                .Include(r => r.BHWProfile)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<Resident> CreateResidentAsync(Resident resident)
        {
            resident.CreatedAt = DateTime.UtcNow;
            _context.Residents.Add(resident);
            await _context.SaveChangesAsync();
            return resident;
        }

        public async Task<Resident?> UpdateResidentAsync(int id, Resident resident)
        {
            var existing = await _context.Residents.FindAsync(id);
            if (existing == null) return null;

            existing.FirstName = resident.FirstName;
            existing.LastName = resident.LastName;
            existing.MiddleName = resident.MiddleName;
            existing.Suffix = resident.Suffix;
            existing.DateOfBirth = resident.DateOfBirth;
            existing.Gender = resident.Gender;
            existing.Address = resident.Address;
            existing.ContactNumber = resident.ContactNumber;
            existing.Email = resident.Email;
            existing.CivilStatus = resident.CivilStatus;
            existing.Occupation = resident.Occupation;
            existing.EmploymentStatus = resident.EmploymentStatus;
            existing.IsVoter = resident.IsVoter;
            existing.VoterId = resident.VoterId;
            existing.HouseholdId = resident.HouseholdId;
            existing.BHWProfileId = resident.BHWProfileId;
            existing.RelationshipToHead = resident.RelationshipToHead;
            existing.EducationalAttainment = resident.EducationalAttainment;
            existing.BloodType = resident.BloodType;
            existing.IsPWD = resident.IsPWD;
            existing.IsSenior = resident.IsSenior;
            existing.Notes = resident.Notes;
            existing.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteResidentAsync(int id)
        {
            var resident = await _context.Residents
                .Include(r => r.Certificates)
                .Include(r => r.MedicalRecords)
                .FirstOrDefaultAsync(r => r.Id == id);
            
            if (resident == null) return false;

            try
            {
                // Check for related records that prevent deletion
                var hasCertificates = await _context.Certificates.AnyAsync(c => c.ResidentId == id);
                var hasMedicalRecords = await _context.MedicalRecords.AnyAsync(m => m.ResidentId == id);
                var hasVaccinations = await _context.Vaccinations.AnyAsync(v => v.ResidentId == id);
                var hasEvacuees = await _context.Evacuees.AnyAsync(e => e.ResidentId == id);
                var hasIncidentsAsComplainant = await _context.Incidents.AnyAsync(i => i.ComplainantId == id);
                var hasIncidentsAsRespondent = await _context.Incidents.AnyAsync(i => i.RespondentId == id);
                var hasSeniorCitizenID = await _context.SeniorCitizenIDs.AnyAsync(s => s.ResidentId == id);

                if (hasCertificates || hasMedicalRecords || hasVaccinations || hasEvacuees || 
                    hasIncidentsAsComplainant || hasIncidentsAsRespondent || hasSeniorCitizenID)
                {
                    // Build detailed error message
                    var relatedRecords = new List<string>();
                    if (hasCertificates) relatedRecords.Add("certificates");
                    if (hasMedicalRecords) relatedRecords.Add("medical records");
                    if (hasVaccinations) relatedRecords.Add("vaccinations");
                    if (hasEvacuees) relatedRecords.Add("evacuee records");
                    if (hasIncidentsAsComplainant || hasIncidentsAsRespondent) relatedRecords.Add("incidents");
                    if (hasSeniorCitizenID) relatedRecords.Add("senior citizen ID");

                    var errorMessage = $"Cannot delete resident because they have related records ({string.Join(", ", relatedRecords)}). Please delete or reassign these records first.";
                    throw new InvalidOperationException(errorMessage);
                }

                _context.Residents.Remove(resident);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
            {
                // Handle foreign key constraint violations
                var errorMessage = ex.InnerException?.Message ?? ex.Message;
                if (errorMessage.Contains("FOREIGN KEY") || errorMessage.Contains("constraint"))
                {
                    throw new InvalidOperationException("Cannot delete resident because they have related records. Please delete or reassign these records first.");
                }
                throw;
            }
        }

        public async Task<bool> ForceDeleteResidentAsync(int id)
        {
            var resident = await _context.Residents
                .Include(r => r.Certificates)
                .Include(r => r.MedicalRecords)
                .FirstOrDefaultAsync(r => r.Id == id);
            
            if (resident == null) return false;

            try
            {
                // Delete related records that can be deleted
                var certificates = await _context.Certificates.Where(c => c.ResidentId == id).ToListAsync();
                _context.Certificates.RemoveRange(certificates);

                var medicalRecords = await _context.MedicalRecords.Where(m => m.ResidentId == id).ToListAsync();
                _context.MedicalRecords.RemoveRange(medicalRecords);

                var vaccinations = await _context.Vaccinations.Where(v => v.ResidentId == id).ToListAsync();
                _context.Vaccinations.RemoveRange(vaccinations);

                var evacuees = await _context.Evacuees.Where(e => e.ResidentId == id).ToListAsync();
                _context.Evacuees.RemoveRange(evacuees);

                // Delete Senior Citizen records (cascade will handle benefits and monitoring)
                var seniorCitizenIDs = await _context.SeniorCitizenIDs.Where(s => s.ResidentId == id).ToListAsync();
                _context.SeniorCitizenIDs.RemoveRange(seniorCitizenIDs);

                // Set null for incidents (they use NoAction, so we need to set to null manually)
                var incidentsAsComplainant = await _context.Incidents.Where(i => i.ComplainantId == id).ToListAsync();
                foreach (var incident in incidentsAsComplainant)
                {
                    incident.ComplainantId = null;
                }

                var incidentsAsRespondent = await _context.Incidents.Where(i => i.RespondentId == id).ToListAsync();
                foreach (var incident in incidentsAsRespondent)
                {
                    incident.RespondentId = null;
                }

                // BHWVisitLog uses SetNull, so it will be handled automatically, but we can set it explicitly
                var visitLogs = await _context.BHWVisitLogs.Where(v => v.ResidentId == id).ToListAsync();
                foreach (var log in visitLogs)
                {
                    log.ResidentId = null;
                }

                // Delete the resident
                _context.Residents.Remove(resident);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error during force delete: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<Household>> GetAllHouseholdsAsync()
        {
            return await _context.Households
                .Include(h => h.Residents)
                .OrderBy(h => h.HouseholdNumber)
                .ToListAsync();
        }

        public async Task<Household?> GetHouseholdByIdAsync(int id)
        {
            return await _context.Households
                .Include(h => h.Residents)
                .FirstOrDefaultAsync(h => h.Id == id);
        }

        public async Task<Household> CreateHouseholdAsync(Household household)
        {
            household.CreatedAt = DateTime.UtcNow;
            _context.Households.Add(household);
            await _context.SaveChangesAsync();
            return household;
        }

        public async Task<IEnumerable<Resident>> GetResidentsByBHWIdAsync(int bhwProfileId, string? search = null)
        {
            var query = _context.Residents
                .Include(r => r.Household)
                .Include(r => r.BHWProfile)
                .Where(r => r.BHWProfileId == bhwProfileId)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();
                query = query.Where(r =>
                    r.FirstName.ToLower().Contains(search) ||
                    r.LastName.ToLower().Contains(search) ||
                    r.MiddleName != null && r.MiddleName.ToLower().Contains(search) ||
                    r.Address.ToLower().Contains(search) ||
                    r.ContactNumber != null && r.ContactNumber.Contains(search));
            }

            return await query.OrderBy(r => r.LastName).ThenBy(r => r.FirstName).ToListAsync();
        }

        public async Task<PagedResult<Resident>> GetResidentsByBHWIdPagedAsync(int bhwProfileId, int pageNumber, int pageSize, string? search = null)
        {
            var query = _context.Residents
                .Include(r => r.Household)
                .Include(r => r.BHWProfile)
                .Where(r => r.BHWProfileId == bhwProfileId)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();
                query = query.Where(r =>
                    r.FirstName.ToLower().Contains(search) ||
                    r.LastName.ToLower().Contains(search) ||
                    r.MiddleName != null && r.MiddleName.ToLower().Contains(search) ||
                    r.Address.ToLower().Contains(search) ||
                    r.ContactNumber != null && r.ContactNumber.Contains(search));
            }

            var totalCount = await query.CountAsync();

            var residents = await query
                .OrderBy(r => r.LastName)
                .ThenBy(r => r.FirstName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Resident>
            {
                Data = residents,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        public async Task<object> GetBHWResidentStatisticsAsync(int bhwProfileId)
        {
            var residents = await _context.Residents
                .Where(r => r.BHWProfileId == bhwProfileId)
                .ToListAsync();

            var totalResidents = residents.Count;
            var currentDate = DateTime.UtcNow;

            var statistics = new
            {
                TotalResidents = totalResidents,
                ByGender = residents.GroupBy(r => r.Gender)
                    .Select(g => new { Gender = g.Key, Count = g.Count() })
                    .ToList(),
                ByAgeGroup = new
                {
                    Children = residents.Count(r => (currentDate.Year - r.DateOfBirth.Year) < 18),
                    Adults = residents.Count(r => (currentDate.Year - r.DateOfBirth.Year) >= 18 && (currentDate.Year - r.DateOfBirth.Year) < 60),
                    Seniors = residents.Count(r => (currentDate.Year - r.DateOfBirth.Year) >= 60 || r.IsSenior)
                },
                SpecialCategories = new
                {
                    PWD = residents.Count(r => r.IsPWD),
                    SeniorCitizens = residents.Count(r => r.IsSenior || (currentDate.Year - r.DateOfBirth.Year) >= 60),
                    Voters = residents.Count(r => r.IsVoter)
                },
                ByCivilStatus = residents
                    .Where(r => !string.IsNullOrEmpty(r.CivilStatus))
                    .GroupBy(r => r.CivilStatus)
                    .Select(g => new { CivilStatus = g.Key, Count = g.Count() })
                    .ToList(),
                ByEmploymentStatus = residents
                    .Where(r => !string.IsNullOrEmpty(r.EmploymentStatus))
                    .GroupBy(r => r.EmploymentStatus)
                    .Select(g => new { EmploymentStatus = g.Key, Count = g.Count() })
                    .ToList(),
                WithContactInfo = new
                {
                    HasPhone = residents.Count(r => !string.IsNullOrEmpty(r.ContactNumber)),
                    HasEmail = residents.Count(r => !string.IsNullOrEmpty(r.Email))
                }
            };

            return statistics;
        }
    }
}



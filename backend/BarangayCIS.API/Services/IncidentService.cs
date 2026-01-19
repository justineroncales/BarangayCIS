using Microsoft.EntityFrameworkCore;
using BarangayCIS.API.Data;
using BarangayCIS.API.Models;

namespace BarangayCIS.API.Services
{
    public class IncidentService : IIncidentService
    {
        private readonly ApplicationDbContext _context;

        public IncidentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Incident>> GetAllIncidentsAsync(string? type = null, string? status = null)
        {
            var query = _context.Incidents
                .Include(i => i.Complainant)
                .Include(i => i.Respondent)
                .Include(i => i.Attachments)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(type))
            {
                query = query.Where(i => i.IncidentType == type);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(i => i.Status == status);
            }

            return await query.OrderByDescending(i => i.ReportedDate).ToListAsync();
        }

        public async Task<Incident?> GetIncidentByIdAsync(int id)
        {
            return await _context.Incidents
                .Include(i => i.Complainant)
                .Include(i => i.Respondent)
                .Include(i => i.Attachments)
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<Incident> CreateIncidentAsync(Incident incident)
        {
            // Validate incident type
            if (string.IsNullOrWhiteSpace(incident.IncidentType))
            {
                throw new ArgumentException("Incident type is required");
            }

            // Validate title
            if (string.IsNullOrWhiteSpace(incident.Title))
            {
                throw new ArgumentException("Title is required");
            }

            // Generate unique incident number
            var year = DateTime.Now.Year;
            var prefix = incident.IncidentType.Length >= 3
                ? incident.IncidentType.Substring(0, 3).ToUpper()
                : incident.IncidentType.ToUpper();
            
            var count = await _context.Incidents
                .CountAsync(i => i.IncidentType == incident.IncidentType && i.ReportedDate.Year == year);
            
            // Generate incident number and ensure uniqueness
            string incidentNumber;
            int attempts = 0;
            do
            {
                incidentNumber = $"{prefix}-{year}-{(count + attempts + 1):D5}";
                var exists = await _context.Incidents
                    .AnyAsync(i => i.IncidentNumber == incidentNumber);
                
                if (!exists) break;
                attempts++;
                
                if (attempts > 100) // Safety limit
                {
                    throw new InvalidOperationException("Unable to generate unique incident number");
                }
            } while (true);

            incident.IncidentNumber = incidentNumber;
            incident.ReportedDate = DateTime.UtcNow;
            incident.CreatedAt = DateTime.UtcNow;
            
            // Set default status if not provided
            if (string.IsNullOrWhiteSpace(incident.Status))
            {
                incident.Status = "Open";
            }

            try
            {
                _context.Incidents.Add(incident);
                await _context.SaveChangesAsync();
                return incident;
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
            {
                var innerException = ex.InnerException?.Message ?? ex.Message;
                throw new InvalidOperationException($"Failed to create incident: {innerException}", ex);
            }
        }

        public async Task<Incident?> UpdateIncidentAsync(int id, Incident incident)
        {
            var existing = await _context.Incidents.FindAsync(id);
            if (existing == null) return null;

            existing.IncidentType = incident.IncidentType;
            existing.Title = incident.Title;
            existing.Description = incident.Description;
            existing.Location = incident.Location;
            existing.ComplainantId = incident.ComplainantId;
            existing.RespondentId = incident.RespondentId;
            existing.ComplainantName = incident.ComplainantName;
            existing.RespondentName = incident.RespondentName;
            existing.Status = incident.Status;
            existing.ActionTaken = incident.ActionTaken;
            existing.Resolution = incident.Resolution;
            existing.ResolutionDate = incident.ResolutionDate;
            existing.MediationScheduledDate = incident.MediationScheduledDate;
            existing.AssignedTo = incident.AssignedTo;
            existing.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteIncidentAsync(int id)
        {
            var incident = await _context.Incidents.FindAsync(id);
            if (incident == null) return false;

            _context.Incidents.Remove(incident);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}



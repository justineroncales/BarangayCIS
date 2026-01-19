using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BarangayCIS.API.Models
{
    public class Incident
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string IncidentType { get; set; } = string.Empty; // Complaint, Blotter, Case, IncidentReport
        
        [StringLength(50)]
        public string IncidentNumber { get; set; } = string.Empty;
        
        public DateTime IncidentDate { get; set; }
        
        public DateTime ReportedDate { get; set; } = DateTime.UtcNow;
        
        [Required]
        [StringLength(255)]
        public string Title { get; set; } = string.Empty;
        
        [StringLength(2000)]
        public string? Description { get; set; }
        
        [StringLength(255)]
        public string? Location { get; set; }
        
        public int? ComplainantId { get; set; }
        
        public int? RespondentId { get; set; }
        
        [StringLength(100)]
        public string? ComplainantName { get; set; }
        
        [StringLength(100)]
        public string? RespondentName { get; set; }
        
        [StringLength(50)]
        public string Status { get; set; } = "Open"; // Open, Under Investigation, Resolved, Closed
        
        [StringLength(2000)]
        public string? ActionTaken { get; set; }
        
        [StringLength(2000)]
        public string? Resolution { get; set; }
        
        public DateTime? ResolutionDate { get; set; }
        
        public DateTime? MediationScheduledDate { get; set; }
        
        [StringLength(100)]
        public string? AssignedTo { get; set; }
        
        [StringLength(100)]
        public string? ReportedBy { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation properties
        [ForeignKey("ComplainantId")]
        public Resident? Complainant { get; set; }
        
        [ForeignKey("RespondentId")]
        public Resident? Respondent { get; set; }
        
        public ICollection<IncidentAttachment> Attachments { get; set; } = new List<IncidentAttachment>();
    }
    
    public class IncidentAttachment
    {
        public int Id { get; set; }
        
        public int IncidentId { get; set; }
        
        [StringLength(255)]
        public string FileName { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string FilePath { get; set; } = string.Empty;
        
        [StringLength(50)]
        public string FileType { get; set; } = string.Empty;
        
        public long FileSize { get; set; }
        
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
        
        [ForeignKey("IncidentId")]
        public Incident Incident { get; set; } = null!;
    }
}



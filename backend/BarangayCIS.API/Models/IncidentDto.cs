using System.ComponentModel.DataAnnotations;

namespace BarangayCIS.API.Models
{
    public class CreateIncidentDto
    {
        [Required]
        [StringLength(50)]
        public string IncidentType { get; set; } = string.Empty;
        
        [Required]
        public DateTime IncidentDate { get; set; }
        
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
        public string Status { get; set; } = "Open";
        
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
    }

    public class UpdateIncidentDto
    {
        [StringLength(50)]
        public string? IncidentType { get; set; }
        
        public DateTime? IncidentDate { get; set; }
        
        [StringLength(255)]
        public string? Title { get; set; }
        
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
        public string? Status { get; set; }
        
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
    }
}




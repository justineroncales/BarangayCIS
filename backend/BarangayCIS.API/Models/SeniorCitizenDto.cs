using System.ComponentModel.DataAnnotations;

namespace BarangayCIS.API.Models
{
    // Senior Citizen ID DTOs
    public class CreateSeniorCitizenIDDto
    {
        [Required]
        public int ResidentId { get; set; }
        
        public DateTime ApplicationDate { get; set; } = DateTime.UtcNow;
        
        public DateTime? IssueDate { get; set; }
        
        public DateTime? ExpiryDate { get; set; }
        
        [StringLength(50)]
        public string Status { get; set; } = "Pending";
        
        [StringLength(500)]
        public string? RequirementsSubmitted { get; set; }
        
        [StringLength(500)]
        public string? RequirementsMissing { get; set; }
        
        [StringLength(500)]
        public string? Remarks { get; set; }
        
        [StringLength(100)]
        public string? ProcessedBy { get; set; }
        
        public DateTime? LastValidatedDate { get; set; }
        
        public DateTime? NextValidationDate { get; set; }
    }
    
    public class UpdateSeniorCitizenIDDto : CreateSeniorCitizenIDDto
    {
        // Inherits all properties
    }
    
    // Senior Citizen Benefit DTOs
    public class CreateSeniorCitizenBenefitDto
    {
        [Required]
        public int SeniorCitizenIDId { get; set; }
        
        [Required]
        [StringLength(50)]
        public string BenefitType { get; set; } = string.Empty;
        
        [StringLength(255)]
        public string? BenefitDescription { get; set; }
        
        public decimal? Amount { get; set; }
        
        public DateTime BenefitDate { get; set; } = DateTime.UtcNow;
        
        [StringLength(50)]
        public string Status { get; set; } = "Pending";
        
        [StringLength(2000)]
        public string? Requirements { get; set; }
        
        [StringLength(2000)]
        public string? Notes { get; set; }
        
        [StringLength(100)]
        public string? ProcessedBy { get; set; }
        
        public DateTime? ProcessedDate { get; set; }
        
        [StringLength(200)]
        public string? ReferenceNumber { get; set; }
        
        [StringLength(100)]
        public string? PaymentMethod { get; set; }
    }
    
    public class UpdateSeniorCitizenBenefitDto : CreateSeniorCitizenBenefitDto
    {
        // Inherits all properties
    }
    
    // Senior Health Monitoring DTOs
    public class CreateSeniorHealthMonitoringDto
    {
        [Required]
        public int SeniorCitizenIDId { get; set; }
        
        public DateTime MonitoringDate { get; set; } = DateTime.UtcNow;
        
        [StringLength(50)]
        public string MonitoringType { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string? BloodPressure { get; set; }
        
        [StringLength(50)]
        public string? BloodSugar { get; set; }
        
        [StringLength(50)]
        public string? Weight { get; set; }
        
        [StringLength(50)]
        public string? Height { get; set; }
        
        [StringLength(50)]
        public string? BMI { get; set; }
        
        [StringLength(2000)]
        public string? HealthFindings { get; set; }
        
        [StringLength(2000)]
        public string? Complaints { get; set; }
        
        [StringLength(2000)]
        public string? Medications { get; set; }
        
        [StringLength(2000)]
        public string? Recommendations { get; set; }
        
        [StringLength(50)]
        public string? ReferralStatus { get; set; }
        
        [StringLength(500)]
        public string? ReferralNotes { get; set; }
        
        [StringLength(100)]
        public string? AttendedBy { get; set; }
        
        [StringLength(500)]
        public string? Notes { get; set; }
        
        public DateTime? NextCheckupDate { get; set; }
    }
    
    public class UpdateSeniorHealthMonitoringDto : CreateSeniorHealthMonitoringDto
    {
        // Inherits all properties
    }
}


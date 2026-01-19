using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BarangayCIS.API.Models
{
    // Senior Citizen ID Registration
    public class SeniorCitizenID
    {
        public int Id { get; set; }
        
        [Required]
        public int ResidentId { get; set; }
        
        [Required]
        [StringLength(50)]
        public string SeniorCitizenNumber { get; set; } = string.Empty; // Unique SC ID number
        
        public DateTime ApplicationDate { get; set; }
        
        public DateTime? IssueDate { get; set; }
        
        public DateTime? ExpiryDate { get; set; } // Usually lifetime, but can be set for validation
        
        [StringLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, Approved, Issued, Expired, Cancelled
        
        [StringLength(500)]
        public string? RequirementsSubmitted { get; set; } // Comma-separated list of submitted requirements
        
        [StringLength(500)]
        public string? RequirementsMissing { get; set; } // Requirements still needed
        
        [StringLength(500)]
        public string? Remarks { get; set; }
        
        [StringLength(100)]
        public string? ProcessedBy { get; set; }
        
        public DateTime? LastValidatedDate { get; set; } // Annual validation
        
        public DateTime? NextValidationDate { get; set; } // Next validation due date
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation property
        [ForeignKey("ResidentId")]
        public Resident Resident { get; set; } = null!;
        
        public ICollection<SeniorCitizenBenefit> Benefits { get; set; } = new List<SeniorCitizenBenefit>();
        public ICollection<SeniorHealthMonitoring> HealthMonitorings { get; set; } = new List<SeniorHealthMonitoring>();
    }
    
    // Senior Citizen Benefits - Social pension, discounts, medical assistance
    public class SeniorCitizenBenefit
    {
        public int Id { get; set; }
        
        [Required]
        public int SeniorCitizenIDId { get; set; }
        
        [Required]
        [StringLength(50)]
        public string BenefitType { get; set; } = string.Empty; // Social Pension, Discount, Medical Assistance, Burial Assistance, etc.
        
        [StringLength(255)]
        public string? BenefitDescription { get; set; }
        
        public decimal? Amount { get; set; } // Amount if applicable
        
        public DateTime BenefitDate { get; set; } // Date benefit was given/applied
        
        [StringLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, Approved, Disbursed, Denied
        
        [StringLength(2000)]
        public string? Requirements { get; set; } // Requirements for the benefit
        
        [StringLength(2000)]
        public string? Notes { get; set; }
        
        [StringLength(100)]
        public string? ProcessedBy { get; set; }
        
        public DateTime? ProcessedDate { get; set; }
        
        [StringLength(200)]
        public string? ReferenceNumber { get; set; } // Transaction reference, check number, etc.
        
        [StringLength(100)]
        public string? PaymentMethod { get; set; } // Cash, Bank Transfer, Check
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation property
        [ForeignKey("SeniorCitizenIDId")]
        public SeniorCitizenID SeniorCitizenID { get; set; } = null!;
    }
    
    // Senior Health Monitoring - Regular health checkups and monitoring
    public class SeniorHealthMonitoring
    {
        public int Id { get; set; }
        
        [Required]
        public int SeniorCitizenIDId { get; set; }
        
        public DateTime MonitoringDate { get; set; }
        
        [StringLength(50)]
        public string MonitoringType { get; set; } = string.Empty; // Regular Checkup, Vaccination, Health Screening, Follow-up
        
        [StringLength(100)]
        public string? BloodPressure { get; set; } // e.g., "120/80"
        
        [StringLength(50)]
        public string? BloodSugar { get; set; } // Fasting/random blood sugar
        
        [StringLength(50)]
        public string? Weight { get; set; } // in kg
        
        [StringLength(50)]
        public string? Height { get; set; } // in cm
        
        [StringLength(50)]
        public string? BMI { get; set; } // Body Mass Index
        
        [StringLength(2000)]
        public string? HealthFindings { get; set; } // General health observations
        
        [StringLength(2000)]
        public string? Complaints { get; set; } // Health complaints from senior
        
        [StringLength(2000)]
        public string? Medications { get; set; } // Current medications
        
        [StringLength(2000)]
        public string? Recommendations { get; set; } // Health recommendations
        
        [StringLength(50)]
        public string? ReferralStatus { get; set; } // None, Referred to Health Center, Referred to Hospital
        
        [StringLength(500)]
        public string? ReferralNotes { get; set; }
        
        [StringLength(100)]
        public string? AttendedBy { get; set; } // BHW, Nurse, Doctor name
        
        [StringLength(500)]
        public string? Notes { get; set; }
        
        public DateTime? NextCheckupDate { get; set; } // Recommended next checkup
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation property
        [ForeignKey("SeniorCitizenIDId")]
        public SeniorCitizenID SeniorCitizenID { get; set; } = null!;
    }
}


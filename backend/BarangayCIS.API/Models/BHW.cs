using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BarangayCIS.API.Models
{
    // BHW Profile - Registration and basic information
    public class BHWProfile
    {
        public int Id { get; set; }
        
        public int? ResidentId { get; set; } // Link to resident if they are a resident
        
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string? MiddleName { get; set; }
        
        [StringLength(20)]
        public string? Suffix { get; set; }
        
        public DateTime DateOfBirth { get; set; }
        
        [StringLength(10)]
        public string Gender { get; set; } = string.Empty;
        
        [Required]
        [StringLength(255)]
        public string Address { get; set; } = string.Empty;
        
        [StringLength(20)]
        public string? ContactNumber { get; set; }
        
        [StringLength(100)]
        public string? Email { get; set; }
        
        [StringLength(50)]
        public string? CivilStatus { get; set; }
        
        [StringLength(100)]
        public string? EducationalAttainment { get; set; }
        
        [StringLength(50)]
        public string BHWNumber { get; set; } = string.Empty; // Unique BHW ID number
        
        public DateTime DateAppointed { get; set; } // Date appointed as BHW
        
        [StringLength(50)]
        public string Status { get; set; } = "Active"; // Active, Inactive, Resigned, Terminated
        
        [StringLength(500)]
        public string? Specialization { get; set; } // e.g., Maternal Health, Child Health, etc.
        
        [StringLength(500)]
        public string? Notes { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation properties
        [ForeignKey("ResidentId")]
        public Resident? Resident { get; set; }
        
        public ICollection<BHWAssignment> Assignments { get; set; } = new List<BHWAssignment>();
        public ICollection<BHWVisitLog> VisitLogs { get; set; } = new List<BHWVisitLog>();
        public ICollection<BHWTraining> Trainings { get; set; } = new List<BHWTraining>();
        public ICollection<BHWIncentive> Incentives { get; set; } = new List<BHWIncentive>();
    }
    
    // BHW Assignment - Assignment to zones/areas (purok/sitio)
    public class BHWAssignment
    {
        public int Id { get; set; }
        
        [Required]
        public int BHWProfileId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string ZoneName { get; set; } = string.Empty; // Purok/Sitio name
        
        [StringLength(255)]
        public string? ZoneDescription { get; set; }
        
        [StringLength(255)]
        public string? CoverageArea { get; set; } // Specific streets/areas
        
        public DateTime AssignmentDate { get; set; }
        
        public DateTime? EndDate { get; set; } // Null if currently assigned
        
        [StringLength(50)]
        public string Status { get; set; } = "Active"; // Active, Completed, Transferred
        
        [StringLength(500)]
        public string? Notes { get; set; }
        
        [StringLength(100)]
        public string? AssignedBy { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation property
        [ForeignKey("BHWProfileId")]
        public BHWProfile BHWProfile { get; set; } = null!;
    }
    
    // BHW Visit Log - Home visits and health monitoring
    public class BHWVisitLog
    {
        public int Id { get; set; }
        
        [Required]
        public int BHWProfileId { get; set; }
        
        public int? ResidentId { get; set; } // Resident visited
        
        [StringLength(100)]
        public string? VisitedPersonName { get; set; } // If not a registered resident
        
        [StringLength(255)]
        public string? Address { get; set; }
        
        public DateTime VisitDate { get; set; }
        
        [StringLength(50)]
        public string VisitType { get; set; } = string.Empty; // Home Visit, Health Check, Family Planning, Vaccination, Follow-up
        
        [StringLength(2000)]
        public string? VisitPurpose { get; set; }
        
        [StringLength(2000)]
        public string? Findings { get; set; } // Health findings, observations
        
        [StringLength(2000)]
        public string? ActionsTaken { get; set; } // What was done during visit
        
        [StringLength(2000)]
        public string? Recommendations { get; set; } // Recommendations for follow-up
        
        [StringLength(50)]
        public string? ReferralStatus { get; set; } // None, Referred to Health Center, Referred to Hospital
        
        [StringLength(500)]
        public string? ReferralNotes { get; set; }
        
        [StringLength(500)]
        public string? Notes { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        [ForeignKey("BHWProfileId")]
        public BHWProfile BHWProfile { get; set; } = null!;
        
        [ForeignKey("ResidentId")]
        public Resident? Resident { get; set; }
    }
    
    // BHW Training - Training records and certifications
    public class BHWTraining
    {
        public int Id { get; set; }
        
        [Required]
        public int BHWProfileId { get; set; }
        
        [Required]
        [StringLength(255)]
        public string TrainingTitle { get; set; } = string.Empty;
        
        [StringLength(2000)]
        public string? Description { get; set; }
        
        [StringLength(100)]
        public string? TrainingProvider { get; set; } // DOH, LGU, NGO, etc.
        
        public DateTime TrainingDate { get; set; }
        
        public DateTime? TrainingEndDate { get; set; }
        
        [StringLength(50)]
        public string? TrainingType { get; set; } // Basic, Advanced, Refresher, Specialized
        
        [StringLength(50)]
        public string Status { get; set; } = "Completed"; // Completed, In Progress, Cancelled
        
        [StringLength(100)]
        public string? CertificateNumber { get; set; }
        
        [StringLength(500)]
        public string? CertificatePath { get; set; } // Path to certificate file
        
        [StringLength(500)]
        public string? Notes { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation property
        [ForeignKey("BHWProfileId")]
        public BHWProfile BHWProfile { get; set; } = null!;
    }
    
    // BHW Incentive - Allowances and benefits tracking
    public class BHWIncentive
    {
        public int Id { get; set; }
        
        [Required]
        public int BHWProfileId { get; set; }
        
        [Required]
        [StringLength(50)]
        public string IncentiveType { get; set; } = string.Empty; // Monthly Allowance, Performance Bonus, Training Allowance, etc.
        
        [Required]
        public decimal Amount { get; set; }
        
        public DateTime IncentiveDate { get; set; } // Date incentive was given
        
        [StringLength(50)]
        public string PaymentStatus { get; set; } = "Pending"; // Pending, Paid, Cancelled
        
        public DateTime? PaymentDate { get; set; }
        
        [StringLength(100)]
        public string? PaymentMethod { get; set; } // Cash, Bank Transfer, Check
        
        [StringLength(200)]
        public string? ReferenceNumber { get; set; } // Check number, transaction reference
        
        [StringLength(500)]
        public string? Remarks { get; set; }
        
        [StringLength(100)]
        public string? ProcessedBy { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation property
        [ForeignKey("BHWProfileId")]
        public BHWProfile BHWProfile { get; set; } = null!;
    }
}


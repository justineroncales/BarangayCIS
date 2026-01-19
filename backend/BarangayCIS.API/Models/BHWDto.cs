using System.ComponentModel.DataAnnotations;

namespace BarangayCIS.API.Models
{
    // BHW Profile DTOs
    public class CreateBHWProfileDto
    {
        public int? ResidentId { get; set; }
        
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
        
        public DateTime DateAppointed { get; set; }
        
        [StringLength(50)]
        public string Status { get; set; } = "Active";
        
        [StringLength(500)]
        public string? Specialization { get; set; }
        
        [StringLength(500)]
        public string? Notes { get; set; }
    }
    
    public class UpdateBHWProfileDto : CreateBHWProfileDto
    {
        // Inherits all properties from CreateBHWProfileDto
    }
    
    // BHW Assignment DTOs
    public class CreateBHWAssignmentDto
    {
        [Required]
        public int BHWProfileId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string ZoneName { get; set; } = string.Empty;
        
        [StringLength(255)]
        public string? ZoneDescription { get; set; }
        
        [StringLength(255)]
        public string? CoverageArea { get; set; }
        
        public DateTime AssignmentDate { get; set; }
        
        public DateTime? EndDate { get; set; }
        
        [StringLength(50)]
        public string Status { get; set; } = "Active";
        
        [StringLength(500)]
        public string? Notes { get; set; }
        
        [StringLength(100)]
        public string? AssignedBy { get; set; }
    }
    
    public class UpdateBHWAssignmentDto : CreateBHWAssignmentDto
    {
        // Inherits all properties
    }
    
    // BHW Visit Log DTOs
    public class CreateBHWVisitLogDto
    {
        [Required]
        public int BHWProfileId { get; set; }
        
        public int? ResidentId { get; set; }
        
        [StringLength(100)]
        public string? VisitedPersonName { get; set; }
        
        [StringLength(255)]
        public string? Address { get; set; }
        
        public DateTime VisitDate { get; set; }
        
        [StringLength(50)]
        public string VisitType { get; set; } = string.Empty;
        
        [StringLength(2000)]
        public string? VisitPurpose { get; set; }
        
        [StringLength(2000)]
        public string? Findings { get; set; }
        
        [StringLength(2000)]
        public string? ActionsTaken { get; set; }
        
        [StringLength(2000)]
        public string? Recommendations { get; set; }
        
        [StringLength(50)]
        public string? ReferralStatus { get; set; }
        
        [StringLength(500)]
        public string? ReferralNotes { get; set; }
        
        [StringLength(500)]
        public string? Notes { get; set; }
    }
    
    public class UpdateBHWVisitLogDto : CreateBHWVisitLogDto
    {
        // Inherits all properties
    }
    
    // BHW Training DTOs
    public class CreateBHWTrainingDto
    {
        [Required]
        public int BHWProfileId { get; set; }
        
        [Required]
        [StringLength(255)]
        public string TrainingTitle { get; set; } = string.Empty;
        
        [StringLength(2000)]
        public string? Description { get; set; }
        
        [StringLength(100)]
        public string? TrainingProvider { get; set; }
        
        public DateTime TrainingDate { get; set; }
        
        public DateTime? TrainingEndDate { get; set; }
        
        [StringLength(50)]
        public string? TrainingType { get; set; }
        
        [StringLength(50)]
        public string Status { get; set; } = "Completed";
        
        [StringLength(100)]
        public string? CertificateNumber { get; set; }
        
        [StringLength(500)]
        public string? CertificatePath { get; set; }
        
        [StringLength(500)]
        public string? Notes { get; set; }
    }
    
    public class UpdateBHWTrainingDto : CreateBHWTrainingDto
    {
        // Inherits all properties
    }
    
    // BHW Incentive DTOs
    public class CreateBHWIncentiveDto
    {
        [Required]
        public int BHWProfileId { get; set; }
        
        [Required]
        [StringLength(50)]
        public string IncentiveType { get; set; } = string.Empty;
        
        [Required]
        public decimal Amount { get; set; }
        
        public DateTime IncentiveDate { get; set; }
        
        [StringLength(50)]
        public string PaymentStatus { get; set; } = "Pending";
        
        public DateTime? PaymentDate { get; set; }
        
        [StringLength(100)]
        public string? PaymentMethod { get; set; }
        
        [StringLength(200)]
        public string? ReferenceNumber { get; set; }
        
        [StringLength(500)]
        public string? Remarks { get; set; }
        
        [StringLength(100)]
        public string? ProcessedBy { get; set; }
    }
    
    public class UpdateBHWIncentiveDto : CreateBHWIncentiveDto
    {
        // Inherits all properties
    }
}


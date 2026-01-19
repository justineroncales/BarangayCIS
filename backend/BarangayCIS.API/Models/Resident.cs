using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BarangayCIS.API.Models
{
    public class Resident
    {
        public int Id { get; set; }
        
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
        
        [StringLength(255)]
        public string Address { get; set; } = string.Empty;
        
        [StringLength(20)]
        public string? ContactNumber { get; set; }
        
        [StringLength(100)]
        public string? Email { get; set; }
        
        [StringLength(50)]
        public string? CivilStatus { get; set; }
        
        [StringLength(100)]
        public string? Occupation { get; set; }
        
        [StringLength(50)]
        public string? EmploymentStatus { get; set; }
        
        public bool IsVoter { get; set; }
        
        [StringLength(50)]
        public string? VoterId { get; set; }
        
        public int? HouseholdId { get; set; }
        
        public int? BHWProfileId { get; set; }
        
        [StringLength(50)]
        public string? RelationshipToHead { get; set; }
        
        [StringLength(50)]
        public string? EducationalAttainment { get; set; }
        
        [StringLength(20)]
        public string? BloodType { get; set; }
        
        public bool IsPWD { get; set; }
        
        public bool IsSenior { get; set; }
        
        [StringLength(500)]
        public string? Notes { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation properties
        [ForeignKey("HouseholdId")]
        public Household? Household { get; set; }
        
        [ForeignKey("BHWProfileId")]
        public BHWProfile? BHWProfile { get; set; }
        
        public ICollection<Certificate> Certificates { get; set; } = new List<Certificate>();
        public ICollection<MedicalRecord> MedicalRecords { get; set; } = new List<MedicalRecord>();
    }
}


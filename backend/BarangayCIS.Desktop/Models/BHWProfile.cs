using System;

namespace BarangayCIS.Desktop.Models
{
    public class BHWProfile
    {
        public int Id { get; set; }
        public int? ResidentId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? MiddleName { get; set; }
        public string? Suffix { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string? ContactNumber { get; set; }
        public string? Email { get; set; }
        public string? CivilStatus { get; set; }
        public string? EducationalAttainment { get; set; }
        public string BHWNumber { get; set; } = string.Empty;
        public DateTime DateAppointed { get; set; }
        public string Status { get; set; } = "Active";
        public string? Specialization { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        
        public string FullName => $"{FirstName} {MiddleName} {LastName}".Replace("  ", " ").Trim();
    }
}

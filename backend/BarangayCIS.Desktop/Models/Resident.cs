using System;

namespace BarangayCIS.Desktop.Models
{
    public class Resident
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? MiddleName { get; set; }
        public string? Suffix { get; set; }
        public DateTime DateOfBirth { get; set; } = DateTime.Now;
        public string Gender { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string? ContactNumber { get; set; }
        public string? Email { get; set; }
        public string? CivilStatus { get; set; }
        public string? Occupation { get; set; }
        public string? EmploymentStatus { get; set; }
        public bool IsVoter { get; set; }
        public string? VoterId { get; set; }
        public int? HouseholdId { get; set; }
        public int? BHWProfileId { get; set; }
        public string? RelationshipToHead { get; set; }
        public string? EducationalAttainment { get; set; }
        public string? BloodType { get; set; }
        public bool IsPWD { get; set; }
        public bool IsSenior { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        
        public string FullName => $"{FirstName} {MiddleName} {LastName}".Replace("  ", " ").Trim();
        public int Age => DateTime.Now.Year - DateOfBirth.Year - (DateTime.Now.DayOfYear < DateOfBirth.DayOfYear ? 1 : 0);
    }
}

using System.ComponentModel.DataAnnotations;

namespace BarangayCIS.API.Models
{
    public class Household
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string HouseholdNumber { get; set; } = string.Empty;
        
        [Required]
        [StringLength(255)]
        public string Address { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string? HeadOfHousehold { get; set; }
        
        public int TotalMembers { get; set; }
        
        [StringLength(50)]
        public string? HousingType { get; set; } // Owned, Rented, etc.
        
        [StringLength(50)]
        public string? TenureStatus { get; set; }
        
        public decimal? MonthlyIncome { get; set; }
        
        [StringLength(50)]
        public string? EconomicStatus { get; set; } // Low, Middle, High
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation properties
        public ICollection<Resident> Residents { get; set; } = new List<Resident>();
    }
}



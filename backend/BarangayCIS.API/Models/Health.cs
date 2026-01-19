using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BarangayCIS.API.Models
{
    public class MedicalRecord
    {
        public int Id { get; set; }
        
        public int ResidentId { get; set; }
        
        [StringLength(50)]
        public string RecordType { get; set; } = string.Empty; // Checkup, Vaccination, Treatment, etc.
        
        public DateTime RecordDate { get; set; }
        
        [StringLength(255)]
        public string? Diagnosis { get; set; }
        
        [StringLength(2000)]
        public string? Symptoms { get; set; }
        
        [StringLength(2000)]
        public string? Treatment { get; set; }
        
        [StringLength(2000)]
        public string? Prescription { get; set; }
        
        [StringLength(100)]
        public string? AttendedBy { get; set; }
        
        [StringLength(500)]
        public string? Notes { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public Resident Resident { get; set; } = null!;
    }
    
    public class Vaccination
    {
        public int Id { get; set; }
        
        public int ResidentId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string VaccineName { get; set; } = string.Empty;
        
        [StringLength(50)]
        public string VaccineType { get; set; } = string.Empty; // COVID-19, Flu, etc.
        
        public DateTime VaccinationDate { get; set; }
        
        [StringLength(50)]
        public string? DoseNumber { get; set; } // 1st, 2nd, Booster, etc.
        
        [StringLength(100)]
        public string? BatchNumber { get; set; }
        
        [StringLength(100)]
        public string? AdministeredBy { get; set; }
        
        public DateTime? NextDoseDate { get; set; }
        
        [StringLength(500)]
        public string? Notes { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public Resident Resident { get; set; } = null!;
    }
    
    public class MedicineInventory
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(255)]
        public string MedicineName { get; set; } = string.Empty;
        
        [StringLength(50)]
        public string? GenericName { get; set; }
        
        [StringLength(50)]
        public string? Unit { get; set; } // tablets, vials, etc.
        
        public int Quantity { get; set; }
        
        public int? MinimumStock { get; set; }
        
        public DateTime? ExpiryDate { get; set; }
        
        [StringLength(100)]
        public string? Supplier { get; set; }
        
        public decimal? UnitPrice { get; set; }
        
        [StringLength(50)]
        public string Status { get; set; } = "Available"; // Available, Low Stock, Expired
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
    }
}



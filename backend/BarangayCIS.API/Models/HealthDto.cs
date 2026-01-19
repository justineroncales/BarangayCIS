using System.ComponentModel.DataAnnotations;

namespace BarangayCIS.API.Models
{
    public class CreateMedicalRecordDto
    {
        [Required]
        public int ResidentId { get; set; }

        [Required]
        [StringLength(50)]
        public string RecordType { get; set; } = string.Empty;

        [Required]
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
    }

    public class UpdateMedicalRecordDto
    {
        public int? ResidentId { get; set; }

        [StringLength(50)]
        public string? RecordType { get; set; }

        public DateTime? RecordDate { get; set; }

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
    }

    public class CreateVaccinationDto
    {
        [Required]
        public int ResidentId { get; set; }

        [Required]
        [StringLength(100)]
        public string VaccineName { get; set; } = string.Empty;

        [StringLength(50)]
        public string VaccineType { get; set; } = string.Empty;

        [Required]
        public DateTime VaccinationDate { get; set; }

        [StringLength(50)]
        public string? DoseNumber { get; set; }

        [StringLength(100)]
        public string? BatchNumber { get; set; }

        [StringLength(100)]
        public string? AdministeredBy { get; set; }

        public DateTime? NextDoseDate { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }
    }

    public class UpdateVaccinationDto
    {
        public int? ResidentId { get; set; }

        [StringLength(100)]
        public string? VaccineName { get; set; }

        [StringLength(50)]
        public string? VaccineType { get; set; }

        public DateTime? VaccinationDate { get; set; }

        [StringLength(50)]
        public string? DoseNumber { get; set; }

        [StringLength(100)]
        public string? BatchNumber { get; set; }

        [StringLength(100)]
        public string? AdministeredBy { get; set; }

        public DateTime? NextDoseDate { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }
    }

    public class CreateMedicineInventoryDto
    {
        [Required]
        [StringLength(255)]
        public string MedicineName { get; set; } = string.Empty;

        [StringLength(50)]
        public string? GenericName { get; set; }

        [StringLength(50)]
        public string? Unit { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }

        public int? MinimumStock { get; set; }

        public DateTime? ExpiryDate { get; set; }

        [StringLength(100)]
        public string? Supplier { get; set; }

        public decimal? UnitPrice { get; set; }

        [StringLength(50)]
        public string Status { get; set; } = "Available";
    }

    public class UpdateMedicineInventoryDto
    {
        [StringLength(255)]
        public string? MedicineName { get; set; }

        [StringLength(50)]
        public string? GenericName { get; set; }

        [StringLength(50)]
        public string? Unit { get; set; }

        [Range(0, int.MaxValue)]
        public int? Quantity { get; set; }

        public int? MinimumStock { get; set; }

        public DateTime? ExpiryDate { get; set; }

        [StringLength(100)]
        public string? Supplier { get; set; }

        public decimal? UnitPrice { get; set; }

        [StringLength(50)]
        public string? Status { get; set; }
    }
}




using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace BarangayCIS.API.Models
{
    // Delivery Record - Birth records logbook
    public class Delivery
    {
        public int Id { get; set; }
        
        [Required]
        [JsonPropertyName("bhwProfileId")]
        public int BHWProfileId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string MotherName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string ChildName { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string? PurokSitio { get; set; }
        
        [Required]
        [StringLength(10)]
        public string Gender { get; set; } = string.Empty; // M or F
        
        [Required]
        public DateTime DateOfBirth { get; set; }
        
        [StringLength(20)]
        public string? TimeOfBirth { get; set; } // e.g., "2:13 AM"
        
        [StringLength(20)]
        public string? Weight { get; set; } // e.g., "2.84 kg"
        
        [StringLength(20)]
        public string? Height { get; set; } // e.g., "51 CM"
        
        [StringLength(255)]
        public string? PlaceOfBirth { get; set; }
        
        [StringLength(10)]
        public string? DeliveryType { get; set; } // CS or NSD
        
        [StringLength(255)]
        public string? BCGAndHepaB { get; set; } // Vaccination details
        
        [StringLength(255)]
        public string? AttendedBy { get; set; } // Doctor/midwife name and details
        
        public int Year { get; set; } // For filtering by year
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation property
        [ForeignKey("BHWProfileId")]
        [JsonIgnore]
        public BHWProfile? BHWProfile { get; set; }
    }
    
    // KRA Report - Key Results Area Report for Family Planning
    public class KRAReport
    {
        public int Id { get; set; }
        
        [Required]
        public int BHWProfileId { get; set; }
        
        [Required]
        public int Year { get; set; }
        
        [Required]
        public int Month { get; set; } // 1-12
        
        // Family Planning Methods by Age Group
        public int PillsPOP_10To14 { get; set; }
        public int PillsPOP_15To19 { get; set; }
        public int PillsPOP_20Plus { get; set; }
        
        public int PillsCOC_10To14 { get; set; }
        public int PillsCOC_15To19 { get; set; }
        public int PillsCOC_20Plus { get; set; }
        
        public int DMPA_10To14 { get; set; }
        public int DMPA_15To19 { get; set; }
        public int DMPA_20Plus { get; set; }
        
        public int Condom_10To14 { get; set; }
        public int Condom_15To19 { get; set; }
        public int Condom_20Plus { get; set; }
        
        public int Implant_10To14 { get; set; }
        public int Implant_15To19 { get; set; }
        public int Implant_20Plus { get; set; }
        
        public int BTL_10To14 { get; set; }
        public int BTL_15To19 { get; set; }
        public int BTL_20Plus { get; set; }
        
        public int LAM_10To14 { get; set; }
        public int LAM_15To19 { get; set; }
        public int LAM_20Plus { get; set; }
        
        public int IUD_10To14 { get; set; }
        public int IUD_15To19 { get; set; }
        public int IUD_20Plus { get; set; }
        
        // Deliveries by Age Group
        public int Deliveries_10To14 { get; set; }
        public int Deliveries_15To19 { get; set; }
        public int Deliveries_20Plus { get; set; }
        
        // Teenage Pregnancies (total, no age breakdown)
        public int TeenagePregnancies { get; set; }
        
        [StringLength(500)]
        public string? Notes { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation property
        [ForeignKey("BHWProfileId")]
        [JsonIgnore]
        public BHWProfile? BHWProfile { get; set; }
    }
}


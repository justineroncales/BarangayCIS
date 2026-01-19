using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BarangayCIS.API.Models
{
    public class Certificate
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string CertificateType { get; set; } = string.Empty; // Clearance, Indigency, Residency, BusinessPermit, ID
        
        [Required]
        [StringLength(50)]
        public string CertificateNumber { get; set; } = string.Empty;
        
        public int ResidentId { get; set; }
        
        [StringLength(100)]
        public string? Purpose { get; set; }
        
        public DateTime IssueDate { get; set; } = DateTime.UtcNow;
        
        public DateTime ExpiryDate { get; set; }
        
        public string? QRCodeData { get; set; }
        
        public string? QRCodeImagePath { get; set; }
        
        [StringLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, Approved, Issued, Expired
        
        [StringLength(100)]
        public string? RequestedBy { get; set; }
        
        [StringLength(100)]
        public string? IssuedBy { get; set; }
        
        public DateTime? PickedUpAt { get; set; }
        
        [StringLength(20)]
        public string? SMSNotificationSent { get; set; } // Yes, No
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        [ForeignKey("ResidentId")]
        public Resident? Resident { get; set; }
    }
}


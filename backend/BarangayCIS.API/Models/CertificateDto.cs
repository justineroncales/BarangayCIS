using System.ComponentModel.DataAnnotations;

namespace BarangayCIS.API.Models
{
    public class CreateCertificateDto
    {
        [Required]
        [StringLength(50)]
        public string CertificateType { get; set; } = string.Empty;
        
        [Required]
        public int ResidentId { get; set; }
        
        [StringLength(100)]
        public string? Purpose { get; set; }
        
        [Required]
        public DateTime IssueDate { get; set; }
        
        [Required]
        public DateTime ExpiryDate { get; set; }
        
        [StringLength(50)]
        public string Status { get; set; } = "Pending";
        
        [StringLength(100)]
        public string? IssuedBy { get; set; }
    }

    public class UpdateCertificateDto
    {
        [StringLength(50)]
        public string? CertificateType { get; set; }
        
        public int? ResidentId { get; set; }
        
        [StringLength(100)]
        public string? Purpose { get; set; }
        
        public DateTime? IssueDate { get; set; }
        
        public DateTime? ExpiryDate { get; set; }
        
        [StringLength(50)]
        public string? Status { get; set; }
        
        [StringLength(100)]
        public string? IssuedBy { get; set; }
        
        public DateTime? PickedUpAt { get; set; }
        
        [StringLength(20)]
        public string? SMSNotificationSent { get; set; }
    }
}



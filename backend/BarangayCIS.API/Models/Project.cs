using System.ComponentModel.DataAnnotations;

namespace BarangayCIS.API.Models
{
    public class Project
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(255)]
        public string ProjectName { get; set; } = string.Empty;
        
        [StringLength(50)]
        public string ProjectNumber { get; set; } = string.Empty;
        
        [StringLength(2000)]
        public string? Description { get; set; }
        
        [StringLength(100)]
        public string? Contractor { get; set; }
        
        public decimal Budget { get; set; }
        
        public decimal? SpentAmount { get; set; }
        
        public DateTime StartDate { get; set; }
        
        public DateTime TargetCompletionDate { get; set; }
        
        public DateTime? ActualCompletionDate { get; set; }
        
        [StringLength(50)]
        public string Status { get; set; } = "Planning"; // Planning, Ongoing, On Hold, Completed, Cancelled
        
        [StringLength(50)]
        public string Progress { get; set; } = "0%";
        
        [StringLength(500)]
        public string? BeforePhotoPath { get; set; }
        
        [StringLength(500)]
        public string? AfterPhotoPath { get; set; }
        
        [StringLength(2000)]
        public string? Notes { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        public ICollection<ProjectUpdate> Updates { get; set; } = new List<ProjectUpdate>();
    }
    
    public class ProjectUpdate
    {
        public int Id { get; set; }
        
        public int ProjectId { get; set; }
        
        [StringLength(2000)]
        public string? UpdateDescription { get; set; }
        
        [StringLength(50)]
        public string? Progress { get; set; }
        
        [StringLength(500)]
        public string? PhotoPath { get; set; }
        
        [StringLength(100)]
        public string? UpdatedBy { get; set; }
        
        public DateTime UpdateDate { get; set; } = DateTime.UtcNow;
        
        public Project Project { get; set; } = null!;
    }
}



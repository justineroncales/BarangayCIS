using System.ComponentModel.DataAnnotations;

namespace BarangayCIS.API.Models
{
    public class CreateProjectDto
    {
        [Required]
        [StringLength(255)]
        public string ProjectName { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Description { get; set; }

        [StringLength(100)]
        public string? Contractor { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Budget { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime TargetCompletionDate { get; set; }

        [StringLength(50)]
        public string Status { get; set; } = "Planning";

        [StringLength(50)]
        public string Progress { get; set; } = "0%";

        [StringLength(2000)]
        public string? Notes { get; set; }
    }

    public class UpdateProjectDto
    {
        [StringLength(255)]
        public string? ProjectName { get; set; }

        [StringLength(2000)]
        public string? Description { get; set; }

        [StringLength(100)]
        public string? Contractor { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal? Budget { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? TargetCompletionDate { get; set; }

        public DateTime? ActualCompletionDate { get; set; }

        [StringLength(50)]
        public string? Status { get; set; }

        [StringLength(50)]
        public string? Progress { get; set; }

        [StringLength(2000)]
        public string? Notes { get; set; }
    }
}




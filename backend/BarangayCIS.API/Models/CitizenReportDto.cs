using System.ComponentModel.DataAnnotations;

namespace BarangayCIS.API.Models
{
    public class CreateCitizenReportDto
    {
        [Required]
        [StringLength(255)]
        public string ReportType { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string Title { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Description { get; set; }

        [StringLength(255)]
        public string? Location { get; set; }

        [StringLength(100)]
        public string? ReporterName { get; set; }

        [StringLength(20)]
        public string? ReporterContact { get; set; }

        [StringLength(50)]
        public string Status { get; set; } = "Pending";

        [StringLength(100)]
        public string? AssignedTo { get; set; }
    }

    public class UpdateCitizenReportDto
    {
        [StringLength(255)]
        public string? ReportType { get; set; }

        [StringLength(255)]
        public string? Title { get; set; }

        [StringLength(2000)]
        public string? Description { get; set; }

        [StringLength(255)]
        public string? Location { get; set; }

        [StringLength(100)]
        public string? ReporterName { get; set; }

        [StringLength(20)]
        public string? ReporterContact { get; set; }

        [StringLength(50)]
        public string? Status { get; set; }

        [StringLength(2000)]
        public string? Resolution { get; set; }

        public DateTime? ResolvedDate { get; set; }

        [StringLength(100)]
        public string? AssignedTo { get; set; }
    }
}




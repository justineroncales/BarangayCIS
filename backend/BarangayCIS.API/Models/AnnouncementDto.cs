using System.ComponentModel.DataAnnotations;

namespace BarangayCIS.API.Models
{
    public class CreateAnnouncementDto
    {
        [Required]
        [StringLength(255)]
        public string Title { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Content { get; set; }

        [StringLength(50)]
        public string Type { get; set; } = "General";

        [StringLength(50)]
        public string Priority { get; set; } = "Normal";

        public DateTime? EventDate { get; set; }

        public DateTime? EventEndDate { get; set; }

        [StringLength(255)]
        public string? Location { get; set; }

        [StringLength(50)]
        public string Status { get; set; } = "Active";

        public bool IsPublished { get; set; } = false;
    }

    public class UpdateAnnouncementDto
    {
        [StringLength(255)]
        public string? Title { get; set; }

        [StringLength(2000)]
        public string? Content { get; set; }

        [StringLength(50)]
        public string? Type { get; set; }

        [StringLength(50)]
        public string? Priority { get; set; }

        public DateTime? EventDate { get; set; }

        public DateTime? EventEndDate { get; set; }

        [StringLength(255)]
        public string? Location { get; set; }

        [StringLength(50)]
        public string? Status { get; set; }

        public bool? IsPublished { get; set; }
    }
}




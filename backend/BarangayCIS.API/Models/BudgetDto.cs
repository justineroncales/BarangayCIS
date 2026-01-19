using System.ComponentModel.DataAnnotations;

namespace BarangayCIS.API.Models
{
    public class CreateBudgetDto
    {
        [Required]
        [StringLength(100)]
        public string BudgetName { get; set; } = string.Empty;

        [StringLength(50)]
        public string BudgetType { get; set; } = string.Empty;

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Allocated amount must be greater than 0")]
        public decimal AllocatedAmount { get; set; }

        [Required]
        public DateTime FiscalYearStart { get; set; }

        [Required]
        public DateTime FiscalYearEnd { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }
    }

    public class UpdateBudgetDto
    {
        [StringLength(100)]
        public string? BudgetName { get; set; }

        [StringLength(50)]
        public string? BudgetType { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Allocated amount must be greater than 0")]
        public decimal? AllocatedAmount { get; set; }

        public DateTime? FiscalYearStart { get; set; }

        public DateTime? FiscalYearEnd { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }
    }
}




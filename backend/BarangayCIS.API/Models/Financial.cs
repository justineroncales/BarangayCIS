using System.ComponentModel.DataAnnotations;

namespace BarangayCIS.API.Models
{
    public class Budget
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string BudgetName { get; set; } = string.Empty;
        
        [StringLength(50)]
        public string BudgetType { get; set; } = string.Empty; // General Fund, Special Fund, etc.
        
        public decimal AllocatedAmount { get; set; }
        
        public decimal UsedAmount { get; set; }
        
        public decimal RemainingAmount => AllocatedAmount - UsedAmount;
        
        public DateTime FiscalYearStart { get; set; }
        
        public DateTime FiscalYearEnd { get; set; }
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
    }
    
    public class Expense
    {
        public int Id { get; set; }
        
        public int BudgetId { get; set; }
        
        [Required]
        [StringLength(255)]
        public string ExpenseName { get; set; } = string.Empty;
        
        [StringLength(50)]
        public string Category { get; set; } = string.Empty; // Office Supplies, Equipment, Services, etc.
        
        public decimal Amount { get; set; }
        
        public DateTime ExpenseDate { get; set; }
        
        [StringLength(100)]
        public string? Vendor { get; set; }
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        [StringLength(100)]
        public string? ApprovedBy { get; set; }
        
        [StringLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, Approved, Paid
        
        [StringLength(500)]
        public string? ReceiptPath { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public Budget Budget { get; set; } = null!;
    }
    
    public class InventoryItem
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(255)]
        public string ItemName { get; set; } = string.Empty;
        
        [StringLength(50)]
        public string Category { get; set; } = string.Empty; // Equipment, Supplies, Medicine, etc.
        
        [StringLength(50)]
        public string? Unit { get; set; } // pcs, boxes, etc.
        
        public int Quantity { get; set; }
        
        public decimal? UnitPrice { get; set; }
        
        [StringLength(100)]
        public string? Location { get; set; }
        
        [StringLength(50)]
        public string Status { get; set; } = "Available"; // Available, In Use, Maintenance, Disposed
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        public DateTime? LastMaintenanceDate { get; set; }
        
        public DateTime? NextMaintenanceDate { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        public ICollection<InventoryBorrowing> Borrowings { get; set; } = new List<InventoryBorrowing>();
    }
    
    public class InventoryBorrowing
    {
        public int Id { get; set; }
        
        public int InventoryItemId { get; set; }
        
        [StringLength(100)]
        public string BorrowedBy { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string? Department { get; set; }
        
        public int Quantity { get; set; }
        
        public DateTime BorrowedDate { get; set; }
        
        public DateTime? ReturnedDate { get; set; }
        
        [StringLength(500)]
        public string? Purpose { get; set; }
        
        [StringLength(50)]
        public string Status { get; set; } = "Borrowed"; // Borrowed, Returned, Overdue
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public InventoryItem InventoryItem { get; set; } = null!;
    }
}



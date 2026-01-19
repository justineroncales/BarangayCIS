using Microsoft.EntityFrameworkCore;
using BarangayCIS.API.Data;
using BarangayCIS.API.Models;

namespace BarangayCIS.API.Services
{
    public class FinancialService : IFinancialService
    {
        private readonly ApplicationDbContext _context;

        public FinancialService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Budget>> GetAllBudgetsAsync()
        {
            return await _context.Budgets
                .Include(b => b.Expenses)
                .OrderByDescending(b => b.FiscalYearStart)
                .ToListAsync();
        }

        public async Task<Budget?> GetBudgetByIdAsync(int id)
        {
            return await _context.Budgets
                .Include(b => b.Expenses)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<Budget> CreateBudgetAsync(Budget budget)
        {
            budget.CreatedAt = DateTime.UtcNow;
            _context.Budgets.Add(budget);
            await _context.SaveChangesAsync();
            return budget;
        }

        public async Task<IEnumerable<Expense>> GetExpensesByBudgetIdAsync(int budgetId)
        {
            return await _context.Expenses
                .Where(e => e.BudgetId == budgetId)
                .OrderByDescending(e => e.ExpenseDate)
                .ToListAsync();
        }

        public async Task<Expense> CreateExpenseAsync(Expense expense)
        {
            expense.CreatedAt = DateTime.UtcNow;
            _context.Expenses.Add(expense);

            // Update budget used amount
            var budget = await _context.Budgets.FindAsync(expense.BudgetId);
            if (budget != null)
            {
                budget.UsedAmount += expense.Amount;
                budget.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return expense;
        }

        public async Task<IEnumerable<InventoryItem>> GetAllInventoryItemsAsync()
        {
            return await _context.InventoryItems
                .Include(i => i.Borrowings)
                .OrderBy(i => i.ItemName)
                .ToListAsync();
        }

        public async Task<InventoryItem> CreateInventoryItemAsync(InventoryItem item)
        {
            item.CreatedAt = DateTime.UtcNow;
            _context.InventoryItems.Add(item);
            await _context.SaveChangesAsync();
            return item;
        }

        public async Task<Budget?> UpdateBudgetAsync(int id, Budget budget)
        {
            var existing = await _context.Budgets.FindAsync(id);
            if (existing == null) return null;

            existing.BudgetName = budget.BudgetName;
            existing.BudgetType = budget.BudgetType;
            existing.AllocatedAmount = budget.AllocatedAmount;
            existing.FiscalYearStart = budget.FiscalYearStart;
            existing.FiscalYearEnd = budget.FiscalYearEnd;
            existing.Description = budget.Description;
            existing.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteBudgetAsync(int id)
        {
            var budget = await _context.Budgets.FindAsync(id);
            if (budget == null) return false;

            _context.Budgets.Remove(budget);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Expense?> UpdateExpenseAsync(int id, Expense expense)
        {
            var existing = await _context.Expenses.FindAsync(id);
            if (existing == null) return null;

            var oldAmount = existing.Amount;
            var budget = await _context.Budgets.FindAsync(existing.BudgetId);

            existing.ExpenseName = expense.ExpenseName;
            existing.Category = expense.Category;
            existing.Amount = expense.Amount;
            existing.ExpenseDate = expense.ExpenseDate;
            existing.Vendor = expense.Vendor;
            existing.Description = expense.Description;
            existing.ApprovedBy = expense.ApprovedBy;
            existing.Status = expense.Status;
            existing.ReceiptPath = expense.ReceiptPath;

            // Update budget used amount
            if (budget != null)
            {
                budget.UsedAmount = budget.UsedAmount - oldAmount + expense.Amount;
                budget.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteExpenseAsync(int id)
        {
            var expense = await _context.Expenses.FindAsync(id);
            if (expense == null) return false;

            // Update budget used amount
            var budget = await _context.Budgets.FindAsync(expense.BudgetId);
            if (budget != null)
            {
                budget.UsedAmount -= expense.Amount;
                budget.UpdatedAt = DateTime.UtcNow;
            }

            _context.Expenses.Remove(expense);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<InventoryItem?> UpdateInventoryItemAsync(int id, InventoryItem item)
        {
            var existing = await _context.InventoryItems.FindAsync(id);
            if (existing == null) return null;

            existing.ItemName = item.ItemName;
            existing.Category = item.Category;
            existing.Unit = item.Unit;
            existing.Quantity = item.Quantity;
            existing.UnitPrice = item.UnitPrice;
            existing.Location = item.Location;
            existing.Status = item.Status;
            existing.Description = item.Description;
            existing.LastMaintenanceDate = item.LastMaintenanceDate;
            existing.NextMaintenanceDate = item.NextMaintenanceDate;
            existing.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteInventoryItemAsync(int id)
        {
            var item = await _context.InventoryItems.FindAsync(id);
            if (item == null) return false;

            _context.InventoryItems.Remove(item);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}


using BarangayCIS.API.Models;

namespace BarangayCIS.API.Services
{
    public interface IFinancialService
    {
        Task<IEnumerable<Budget>> GetAllBudgetsAsync();
        Task<Budget?> GetBudgetByIdAsync(int id);
        Task<Budget> CreateBudgetAsync(Budget budget);
        Task<Budget?> UpdateBudgetAsync(int id, Budget budget);
        Task<bool> DeleteBudgetAsync(int id);
        Task<IEnumerable<Expense>> GetExpensesByBudgetIdAsync(int budgetId);
        Task<Expense> CreateExpenseAsync(Expense expense);
        Task<Expense?> UpdateExpenseAsync(int id, Expense expense);
        Task<bool> DeleteExpenseAsync(int id);
        Task<IEnumerable<InventoryItem>> GetAllInventoryItemsAsync();
        Task<InventoryItem> CreateInventoryItemAsync(InventoryItem item);
        Task<InventoryItem?> UpdateInventoryItemAsync(int id, InventoryItem item);
        Task<bool> DeleteInventoryItemAsync(int id);
    }
}


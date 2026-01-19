using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BarangayCIS.API.Services;
using BarangayCIS.API.Models;

namespace BarangayCIS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BudgetsController : ControllerBase
    {
        private readonly IFinancialService _financialService;

        public BudgetsController(IFinancialService financialService)
        {
            _financialService = financialService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var budgets = await _financialService.GetAllBudgetsAsync();
            return Ok(budgets);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var budget = await _financialService.GetBudgetByIdAsync(id);
            if (budget == null) return NotFound();
            return Ok(budget);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateBudgetDto budgetDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var budget = new Budget
                {
                    BudgetName = budgetDto.BudgetName,
                    BudgetType = budgetDto.BudgetType,
                    AllocatedAmount = budgetDto.AllocatedAmount,
                    FiscalYearStart = budgetDto.FiscalYearStart,
                    FiscalYearEnd = budgetDto.FiscalYearEnd,
                    Description = budgetDto.Description,
                    UsedAmount = 0
                };

                var created = await _financialService.CreateBudgetAsync(budget);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the budget", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateBudgetDto budgetDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var existing = await _financialService.GetBudgetByIdAsync(id);
                if (existing == null) return NotFound();

                var budget = new Budget
                {
                    BudgetName = budgetDto.BudgetName ?? existing.BudgetName,
                    BudgetType = budgetDto.BudgetType ?? existing.BudgetType,
                    AllocatedAmount = budgetDto.AllocatedAmount ?? existing.AllocatedAmount,
                    FiscalYearStart = budgetDto.FiscalYearStart ?? existing.FiscalYearStart,
                    FiscalYearEnd = budgetDto.FiscalYearEnd ?? existing.FiscalYearEnd,
                    Description = budgetDto.Description ?? existing.Description,
                    UsedAmount = existing.UsedAmount
                };

                var updated = await _financialService.UpdateBudgetAsync(id, budget);
                if (updated == null) return NotFound();
                return Ok(updated);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the budget", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _financialService.DeleteBudgetAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }
    }

    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ExpensesController : ControllerBase
    {
        private readonly IFinancialService _financialService;

        public ExpensesController(IFinancialService financialService)
        {
            _financialService = financialService;
        }

        [HttpGet("budget/{budgetId}")]
        public async Task<IActionResult> GetByBudgetId(int budgetId)
        {
            var expenses = await _financialService.GetExpensesByBudgetIdAsync(budgetId);
            return Ok(expenses);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Expense expense)
        {
            var created = await _financialService.CreateExpenseAsync(expense);
            return Ok(created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Expense expense)
        {
            var updated = await _financialService.UpdateExpenseAsync(id, expense);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _financialService.DeleteExpenseAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }
    }

    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class InventoryController : ControllerBase
    {
        private readonly IFinancialService _financialService;

        public InventoryController(IFinancialService financialService)
        {
            _financialService = financialService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var items = await _financialService.GetAllInventoryItemsAsync();
            return Ok(items);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] InventoryItem item)
        {
            var created = await _financialService.CreateInventoryItemAsync(item);
            return Ok(created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] InventoryItem item)
        {
            var updated = await _financialService.UpdateInventoryItemAsync(id, item);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _financialService.DeleteInventoryItemAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }
    }
}


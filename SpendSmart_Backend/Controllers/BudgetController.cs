using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SpendSmart_Backend.Data;
using SpendSmart_Backend.DTOs;
using SpendSmart_Backend.Services;
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

namespace SpendSmart_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BudgetController : ControllerBase
    {
        private readonly BudgetService _budgetService;
        private readonly ApplicationDbContext _context;

        public BudgetController(BudgetService budgetService, ApplicationDbContext context)
        {
            _budgetService = budgetService;
            _context = context;
        }

        // GET: api/Budget/debug
        [HttpGet("debug")]
        public ActionResult<string> GetDebugInfo()
        {
            var budgetType = _context.Model.FindEntityType(typeof(SpendSmart_Backend.Models.Budget));
            var properties = budgetType.GetProperties().Select(p => new
            {
                Name = p.Name,
                Type = p.ClrType.Name,
                IsKey = p.IsKey()
            });

            return Ok(new
            {
                BudgetProperties = properties,
                HasUser = _context.Users.Any(),
                UserCount = _context.Users.Count(),
                HasCategories = _context.Categories.Any(),
                CategoryCount = _context.Categories.Count()
            });
        }

        // GET: api/Budget/user/{userId}
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<BudgetSummaryDto>>> GetUserBudgets(int userId)
        {
            var budgets = await _budgetService.GetUserBudgetsAsync(userId);
            return Ok(budgets);
        }

        // GET: api/Budget/details/{budgetId}
        [HttpGet("details/{budgetId}")]
        public async Task<ActionResult<BudgetResponseDto>> GetBudgetDetails(int budgetId)
        {
            var budget = await _budgetService.GetBudgetDetailsAsync(budgetId);
            if (budget == null)
                return NotFound();

            return Ok(budget);
        }

        // GET: api/Budget/{budgetId}/transactions
        [HttpGet("{budgetId}/transactions")]
        public async Task<ActionResult<IEnumerable<TransactionDetailsDto>>> GetBudgetTransactions(int budgetId)
        {
            var transactions = await _budgetService.GetBudgetTransactionsAsync(budgetId);
            return Ok(transactions);
        }

        // GET: api/Budget/{budgetId}/expense-breakdown
        [HttpGet("{budgetId}/expense-breakdown")]
        public async Task<ActionResult<IEnumerable<ExpenseBreakdownDto>>> GetExpenseBreakdown(int budgetId)
        {
            var breakdown = await _budgetService.GetExpenseBreakdownAsync(budgetId);
            return Ok(breakdown);
        }

        // GET: api/Budget/{budgetId}/period-data
        [HttpGet("{budgetId}/period-data")]
        public async Task<ActionResult<IEnumerable<PeriodDataDto>>> GetBudgetPeriodData(int budgetId)
        {
            var periodData = await _budgetService.GetBudgetPeriodDataAsync(budgetId);
            return Ok(periodData);
        }

        // POST: api/Budget/create/{userId}
        [HttpPost("create/{userId}")]
        public async Task<ActionResult<BudgetResponseDto>> CreateBudget(int userId, CreateBudgetDto createBudgetDto)
        {
            try
            {
                var budget = await _budgetService.CreateBudgetAsync(userId, createBudgetDto);
                return CreatedAtAction(nameof(GetBudgetDetails), new { budgetId = budget.BudgetId }, budget);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // PUT: api/Budget/update/{budgetId}
        [HttpPut("update/{budgetId}")]
        public async Task<ActionResult<BudgetResponseDto>> UpdateBudget(int budgetId, UpdateBudgetDto updateBudgetDto)
        {
            try
            {
                var budget = await _budgetService.UpdateBudgetAsync(budgetId, updateBudgetDto);
                if (budget == null)
                    return NotFound();

                return Ok(budget);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // DELETE: api/Budget/delete/{budgetId}
        [HttpDelete("delete/{budgetId}")]
        public async Task<IActionResult> DeleteBudget(int budgetId)
        {
            var result = await _budgetService.DeleteBudgetAsync(budgetId);
            if (!result)
                return NotFound();

            return NoContent();
        }

        // PUT: api/Budget/{budgetId}/status/{status}
        [HttpPut("{budgetId}/status/{status}")]
        public async Task<IActionResult> UpdateBudgetStatus(int budgetId, string status)
        {
            var result = await _budgetService.UpdateBudgetStatusAsync(budgetId, status);
            if (!result)
                return BadRequest("Invalid budget ID or status");

            return NoContent();
        }
        
        // POST: api/Budget/validate-request/{userId}
        [HttpPost("validate-request/{userId}")]
        public IActionResult ValidateBudgetRequest(int userId, [FromBody] CreateBudgetDto createBudgetDto)
        {
            // Check if user exists
            var userExists = _context.Users.Any(u => u.Id == userId);
            if (!userExists)
            {
                return BadRequest(new { Error = $"User with ID {userId} does not exist" });
            }
            
            // Get all available categories
            var availableCategories = _context.Categories.ToList();
            var availableCategoryIds = availableCategories.Select(c => c.Id).ToList();
            
            // Check if all category IDs in the request exist
            var requestedCategoryIds = createBudgetDto.CategoryAllocations.Select(ca => ca.CategoryId).ToList();
            var invalidCategoryIds = requestedCategoryIds.Where(id => !availableCategoryIds.Contains(id)).ToList();
            
            if (invalidCategoryIds.Any())
            {
                return BadRequest(new
                {
                    Error = "Invalid category IDs in request",
                    InvalidCategoryIds = invalidCategoryIds,
                    AvailableCategoryIds = availableCategoryIds,
                    AvailableCategories = availableCategories.Select(c => new { c.Id, c.CategoryName, c.Type })
                });
            }
            
            return Ok(new
            {
                Message = "Budget request is valid",
                UserId = userId,
                BudgetName = createBudgetDto.BudgetName,
                BudgetType = createBudgetDto.BudgetType,
                StartDate = createBudgetDto.StartDate,
                CategoryAllocations = createBudgetDto.CategoryAllocations.Select(ca => new
                {
                    CategoryId = ca.CategoryId,
                    CategoryName = availableCategories.First(c => c.Id == ca.CategoryId).CategoryName,
                    CategoryType = availableCategories.First(c => c.Id == ca.CategoryId).Type,
                    AllocatedAmount = ca.AllocatedAmount
                }),
                TotalAmount = createBudgetDto.CategoryAllocations.Sum(ca => ca.AllocatedAmount)
            });
        }

        // POST: api/Budget/transaction-impact
        [HttpPost("transaction-impact")]
        public async Task<IActionResult> RecordTransactionImpact([FromBody] TransactionImpactDto impactDto)
        {
            await _budgetService.RecordTransactionImpactAsync(
                impactDto.TransactionId,
                impactDto.BudgetId,
                impactDto.CategoryId,
                impactDto.Amount);

            return NoContent();
        }
    }

    public class TransactionImpactDto
    {
        public int TransactionId { get; set; }
        public int BudgetId { get; set; }
        public int CategoryId { get; set; }
        public decimal Amount { get; set; }
    }
} 
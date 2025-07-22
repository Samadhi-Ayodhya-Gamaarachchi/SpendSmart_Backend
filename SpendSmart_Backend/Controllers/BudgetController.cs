using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SpendSmart_Backend.Data;
using SpendSmart_Backend.Models;
using SpendSmart_Backend.DTOs;
using SpendSmart_Backend.DTOs.Common;

namespace SpendSmart_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BudgetController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<BudgetController> _logger;

        public BudgetController(ApplicationDbContext context, ILogger<BudgetController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/Budget/user/{userId}
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<ApiResponseDto<List<BudgetSummaryDto>>>> GetUserBudgets(int userId)
        {
            try
            {
                var budgets = await _context.Budgets
                    .Where(b => b.UserId == userId)
                    .Include(b => b.BudgetCategories)
                    .ThenInclude(bc => bc.Category)
                    .OrderByDescending(b => b.CreatedAt)
                    .Select(b => new BudgetSummaryDto
                    {
                        BudgetId = b.BudgetId,
                        BudgetName = b.BudgetName,
                        BudgetType = b.BudgetType,
                        StartDate = b.StartDate,
                        EndDate = b.EndDate,
                        TotalBudgetAmount = b.TotalBudgetAmount,
                        TotalSpentAmount = b.TotalSpentAmount,
                        RemainingAmount = b.RemainingAmount,
                        ProgressPercentage = b.ProgressPercentage,
                        Status = b.Status
                    })
                    .ToListAsync();

                return Ok(ApiResponseDto<List<BudgetSummaryDto>>.SuccessResponse(budgets));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving budgets for user {UserId}", userId);
                return StatusCode(500, ApiResponseDto<List<BudgetSummaryDto>>.ErrorResponse("An error occurred while retrieving budgets"));
            }
        }

        // GET: api/Budget/{budgetId}
        [HttpGet("{budgetId}")]
        public async Task<ActionResult<ApiResponseDto<BudgetResponseDto>>> GetBudgetDetails(int budgetId)
        {
            try
            {
                var budget = await _context.Budgets
                    .Include(b => b.BudgetCategories)
                    .ThenInclude(bc => bc.Category)
                    .FirstOrDefaultAsync(b => b.BudgetId == budgetId);

                if (budget == null)
                {
                    return NotFound(ApiResponseDto<BudgetResponseDto>.ErrorResponse("Budget not found"));
                }

                var budgetDto = new BudgetResponseDto
                {
                    BudgetId = budget.BudgetId,
                    UserId = budget.UserId,
                    BudgetName = budget.BudgetName,
                    BudgetType = budget.BudgetType,
                    StartDate = budget.StartDate,
                    EndDate = budget.EndDate,
                    TotalBudgetAmount = budget.TotalBudgetAmount,
                    TotalSpentAmount = budget.TotalSpentAmount,
                    RemainingAmount = budget.RemainingAmount,
                    ProgressPercentage = budget.ProgressPercentage,
                    Status = budget.Status,
                    Description = budget.Description,
                    CreatedAt = budget.CreatedAt,
                    UpdatedAt = budget.UpdatedAt,
                    Categories = budget.BudgetCategories.Select(bc => new BudgetCategoryResponseDto
                    {
                        BudgetCategoryId = bc.BudgetCategoryId,
                        CategoryId = bc.CategoryId,
                        CategoryName = bc.Category.CategoryName,
                        AllocatedAmount = bc.AllocatedAmount,
                        SpentAmount = bc.SpentAmount,
                        RemainingAmount = bc.RemainingAmount,
                        CreatedAt = bc.CreatedAt,
                        UpdatedAt = bc.UpdatedAt
                    }).ToList()
                };

                return Ok(ApiResponseDto<BudgetResponseDto>.SuccessResponse(budgetDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving budget details for budget {BudgetId}", budgetId);
                return StatusCode(500, ApiResponseDto<BudgetResponseDto>.ErrorResponse("An error occurred while retrieving budget details"));
            }
        }

        // POST: api/Budget
        [HttpPost]
        public async Task<ActionResult<ApiResponseDto<BudgetResponseDto>>> CreateBudget(CreateBudgetRequestDto request, [FromQuery] int userId)
        {
            try
            {
                using var transaction = await _context.Database.BeginTransactionAsync();

                // Validate categories exist
                var categoryIds = request.CategoryAllocations.Select(ca => ca.CategoryId).ToList();
                var existingCategories = await _context.Categories
                    .Where(c => categoryIds.Contains(c.Id))
                    .ToListAsync();

                if (existingCategories.Count != categoryIds.Count)
                {
                    return BadRequest(ApiResponseDto<BudgetResponseDto>.ErrorResponse("One or more categories do not exist"));
                }

                // Calculate total budget amount
                var totalBudgetAmount = request.CategoryAllocations.Sum(ca => ca.AllocatedAmount);

                // Create budget
                var budget = new Budget
                {
                    UserId = userId,
                    BudgetName = request.BudgetName,
                    BudgetType = request.BudgetType,
                    StartDate = request.StartDate.Date,
                    EndDate = Budget.CalculateEndDate(request.StartDate.Date, request.BudgetType),
                    TotalBudgetAmount = totalBudgetAmount,
                    Description = request.Description,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Budgets.Add(budget);
                await _context.SaveChangesAsync();

                // Create budget categories
                var budgetCategories = request.CategoryAllocations.Select(ca => new BudgetCategory
                {
                    BudgetId = budget.BudgetId,
                    CategoryId = ca.CategoryId,
                    AllocatedAmount = ca.AllocatedAmount,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }).ToList();

                _context.BudgetCategories.AddRange(budgetCategories);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                // Return created budget
                var budgetDto = new BudgetResponseDto
                {
                    BudgetId = budget.BudgetId,
                    UserId = budget.UserId,
                    BudgetName = budget.BudgetName,
                    BudgetType = budget.BudgetType,
                    StartDate = budget.StartDate,
                    EndDate = budget.EndDate,
                    TotalBudgetAmount = budget.TotalBudgetAmount,
                    TotalSpentAmount = budget.TotalSpentAmount,
                    RemainingAmount = budget.RemainingAmount,
                    ProgressPercentage = budget.ProgressPercentage,
                    Status = budget.Status,
                    Description = budget.Description,
                    CreatedAt = budget.CreatedAt,
                    UpdatedAt = budget.UpdatedAt,
                    Categories = budgetCategories.Select(bc => new BudgetCategoryResponseDto
                    {
                        BudgetCategoryId = bc.BudgetCategoryId,
                        CategoryId = bc.CategoryId,
                        CategoryName = existingCategories.First(c => c.Id == bc.CategoryId).CategoryName,
                        AllocatedAmount = bc.AllocatedAmount,
                        SpentAmount = bc.SpentAmount,
                        RemainingAmount = bc.RemainingAmount,
                        CreatedAt = bc.CreatedAt,
                        UpdatedAt = bc.UpdatedAt
                    }).ToList()
                };

                return CreatedAtAction(nameof(GetBudgetDetails), new { budgetId = budget.BudgetId },
                    ApiResponseDto<BudgetResponseDto>.SuccessResponse(budgetDto, "Budget created successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating budget for user {UserId}", userId);
                return StatusCode(500, ApiResponseDto<BudgetResponseDto>.ErrorResponse("An error occurred while creating the budget"));
            }
        }

        // PUT: api/Budget/{budgetId}
        [HttpPut("{budgetId}")]
        public async Task<ActionResult<ApiResponseDto<BudgetResponseDto>>> UpdateBudget(int budgetId, UpdateBudgetRequestDto request)
        {
            try
            {
                var budget = await _context.Budgets
                    .Include(b => b.BudgetCategories)
                    .ThenInclude(bc => bc.Category)
                    .FirstOrDefaultAsync(b => b.BudgetId == budgetId);

                if (budget == null)
                {
                    return NotFound(ApiResponseDto<BudgetResponseDto>.ErrorResponse("Budget not found"));
                }

                using var transaction = await _context.Database.BeginTransactionAsync();

                // Update budget properties
                if (!string.IsNullOrEmpty(request.BudgetName))
                    budget.BudgetName = request.BudgetName;

                if (!string.IsNullOrEmpty(request.Description))
                    budget.Description = request.Description;

                if (!string.IsNullOrEmpty(request.Status))
                    budget.Status = request.Status;

                // Update category allocations if provided
                if (request.CategoryAllocations != null && request.CategoryAllocations.Any())
                {
                    // Remove existing category allocations
                    _context.BudgetCategories.RemoveRange(budget.BudgetCategories);

                    // Add new category allocations
                    var newBudgetCategories = request.CategoryAllocations.Select(ca => new BudgetCategory
                    {
                        BudgetId = budget.BudgetId,
                        CategoryId = ca.CategoryId,
                        AllocatedAmount = ca.AllocatedAmount,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    }).ToList();

                    _context.BudgetCategories.AddRange(newBudgetCategories);

                    // Recalculate total budget amount
                    budget.TotalBudgetAmount = request.CategoryAllocations.Sum(ca => ca.AllocatedAmount);
                    budget.BudgetCategories = newBudgetCategories;
                }

                budget.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Return updated budget
                var budgetDto = new BudgetResponseDto
                {
                    BudgetId = budget.BudgetId,
                    UserId = budget.UserId,
                    BudgetName = budget.BudgetName,
                    BudgetType = budget.BudgetType,
                    StartDate = budget.StartDate,
                    EndDate = budget.EndDate,
                    TotalBudgetAmount = budget.TotalBudgetAmount,
                    TotalSpentAmount = budget.TotalSpentAmount,
                    RemainingAmount = budget.RemainingAmount,
                    ProgressPercentage = budget.ProgressPercentage,
                    Status = budget.Status,
                    Description = budget.Description,
                    CreatedAt = budget.CreatedAt,
                    UpdatedAt = budget.UpdatedAt,
                    Categories = budget.BudgetCategories.Select(bc => new BudgetCategoryResponseDto
                    {
                        BudgetCategoryId = bc.BudgetCategoryId,
                        CategoryId = bc.CategoryId,
                        CategoryName = bc.Category.CategoryName,
                        AllocatedAmount = bc.AllocatedAmount,
                        SpentAmount = bc.SpentAmount,
                        RemainingAmount = bc.RemainingAmount,
                        CreatedAt = bc.CreatedAt,
                        UpdatedAt = bc.UpdatedAt
                    }).ToList()
                };

                return Ok(ApiResponseDto<BudgetResponseDto>.SuccessResponse(budgetDto, "Budget updated successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating budget {BudgetId}", budgetId);
                return StatusCode(500, ApiResponseDto<BudgetResponseDto>.ErrorResponse("An error occurred while updating the budget"));
            }
        }

        // DELETE: api/Budget/{budgetId}
        [HttpDelete("{budgetId}")]
        public async Task<ActionResult<ApiResponseDto<object>>> DeleteBudget(int budgetId)
        {
            try
            {
                var budget = await _context.Budgets
                    .Include(b => b.BudgetCategories)
                    .Include(b => b.TransactionBudgetImpacts)
                    .FirstOrDefaultAsync(b => b.BudgetId == budgetId);

                if (budget == null)
                {
                    return NotFound(ApiResponseDto<object>.ErrorResponse("Budget not found"));
                }

                using var transaction = await _context.Database.BeginTransactionAsync();

                // Remove related data
                _context.TransactionBudgetImpacts.RemoveRange(budget.TransactionBudgetImpacts);
                _context.BudgetCategories.RemoveRange(budget.BudgetCategories);
                _context.Budgets.Remove(budget);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(ApiResponseDto<object>.SuccessResponse(null, "Budget deleted successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting budget {BudgetId}", budgetId);
                return StatusCode(500, ApiResponseDto<object>.ErrorResponse("An error occurred while deleting the budget"));
            }
        }

        // GET: api/Budget/user/{userId}/active
        [HttpGet("user/{userId}/active")]
        public async Task<ActionResult<ApiResponseDto<List<BudgetSummaryDto>>>> GetActiveBudgets(int userId)
        {
            try
            {
                var currentDate = DateTime.UtcNow.Date;
                var activeBudgets = await _context.Budgets
                    .Where(b => b.UserId == userId &&
                               b.Status == "Active" &&
                               b.StartDate <= currentDate &&
                               b.EndDate >= currentDate)
                    .Select(b => new BudgetSummaryDto
                    {
                        BudgetId = b.BudgetId,
                        BudgetName = b.BudgetName,
                        BudgetType = b.BudgetType,
                        StartDate = b.StartDate,
                        EndDate = b.EndDate,
                        TotalBudgetAmount = b.TotalBudgetAmount,
                        TotalSpentAmount = b.TotalSpentAmount,
                        RemainingAmount = b.RemainingAmount,
                        ProgressPercentage = b.ProgressPercentage,
                        Status = b.Status
                    })
                    .ToListAsync();

                return Ok(ApiResponseDto<List<BudgetSummaryDto>>.SuccessResponse(activeBudgets));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active budgets for user {UserId}", userId);
                return StatusCode(500, ApiResponseDto<List<BudgetSummaryDto>>.ErrorResponse("An error occurred while retrieving active budgets"));
            }
        }
    }
}
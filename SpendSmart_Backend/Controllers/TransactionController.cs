using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SpendSmart_Backend.Data;
using SpendSmart_Backend.Models;
using SpendSmart_Backend.DTOs;
using SpendSmart_Backend.DTOs.Common;
using SpendSmart_Backend.Services;

namespace SpendSmart_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IBudgetService _budgetService;
        private readonly ILogger<TransactionController> _logger;

        public TransactionController(
            ApplicationDbContext context,
            IBudgetService budgetService,
            ILogger<TransactionController> logger)
        {
            _context = context;
            _budgetService = budgetService;
            _logger = logger;
        }

        // GET: api/Transaction/user/{userId}
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<ApiResponseDto<List<TransactionResponseDto>>>> GetUserTransactions(
            int userId,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string? transactionType = null,
            [FromQuery] int? categoryId = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            try
            {
                var query = _context.Transactions
                    .Where(t => t.UserId == userId)
                    .Include(t => t.Category)
                    .Include(t => t.TransactionBudgetImpacts)
                    .ThenInclude(tbi => tbi.Budget)
                    .AsQueryable();

                // Apply filters
                if (startDate.HasValue)
                    query = query.Where(t => t.TransactionDate >= startDate.Value.Date);

                if (endDate.HasValue)
                    query = query.Where(t => t.TransactionDate <= endDate.Value.Date);

                if (!string.IsNullOrEmpty(transactionType))
                    query = query.Where(t => t.TransactionType == transactionType);

                if (categoryId.HasValue)
                    query = query.Where(t => t.CategoryId == categoryId.Value);

                // Apply pagination
                var totalCount = await query.CountAsync();
                var transactions = await query
                    .OrderByDescending(t => t.TransactionDate)
                    .ThenByDescending(t => t.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(t => new TransactionResponseDto
                    {
                        TransactionId = t.TransactionId,
                        UserId = t.UserId,
                        CategoryId = t.CategoryId,
                        CategoryName = t.Category.CategoryName,
                        TransactionType = t.TransactionType,
                        Amount = t.Amount,
                        Description = t.Description,
                        TransactionDate = t.TransactionDate,
                        IsRecurring = t.IsRecurring,
                        RecurringFrequency = t.RecurringFrequency,
                        RecurringEndDate = t.RecurringEndDate,
                        CreatedAt = t.CreatedAt,
                        UpdatedAt = t.UpdatedAt,
                        BudgetImpacts = t.TransactionBudgetImpacts.Select(tbi => new BudgetImpactDto
                        {
                            BudgetId = tbi.BudgetId,
                            BudgetName = tbi.Budget.BudgetName,
                            ImpactAmount = tbi.ImpactAmount
                        }).ToList()
                    })
                    .ToListAsync();

                var response = new
                {
                    Transactions = transactions,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                };

                return Ok(ApiResponseDto<object>.SuccessResponse(response));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving transactions for user {UserId}", userId);
                return StatusCode(500, ApiResponseDto<List<TransactionResponseDto>>.ErrorResponse("An error occurred while retrieving transactions"));
            }
        }

        // GET: api/Transaction/{transactionId}
        [HttpGet("{transactionId}")]
        public async Task<ActionResult<ApiResponseDto<TransactionResponseDto>>> GetTransactionById(int transactionId)
        {
            try
            {
                var transaction = await _context.Transactions
                    .Include(t => t.Category)
                    .Include(t => t.TransactionBudgetImpacts)
                    .ThenInclude(tbi => tbi.Budget)
                    .FirstOrDefaultAsync(t => t.TransactionId == transactionId);

                if (transaction == null)
                {
                    return NotFound(ApiResponseDto<TransactionResponseDto>.ErrorResponse("Transaction not found"));
                }

                var transactionDto = new TransactionResponseDto
                {
                    TransactionId = transaction.TransactionId,
                    UserId = transaction.UserId,
                    CategoryId = transaction.CategoryId,
                    CategoryName = transaction.Category.CategoryName,
                    TransactionType = transaction.TransactionType,
                    Amount = transaction.Amount,
                    Description = transaction.Description,
                    TransactionDate = transaction.TransactionDate,
                    IsRecurring = transaction.IsRecurring,
                    RecurringFrequency = transaction.RecurringFrequency,
                    RecurringEndDate = transaction.RecurringEndDate,
                    CreatedAt = transaction.CreatedAt,
                    UpdatedAt = transaction.UpdatedAt,
                    BudgetImpacts = transaction.TransactionBudgetImpacts.Select(tbi => new BudgetImpactDto
                    {
                        BudgetId = tbi.BudgetId,
                        BudgetName = tbi.Budget.BudgetName,
                        ImpactAmount = tbi.ImpactAmount
                    }).ToList()
                };

                return Ok(ApiResponseDto<TransactionResponseDto>.SuccessResponse(transactionDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving transaction {TransactionId}", transactionId);
                return StatusCode(500, ApiResponseDto<TransactionResponseDto>.ErrorResponse("An error occurred while retrieving the transaction"));
            }
        }

        // POST: api/Transaction
        [HttpPost]
        public async Task<ActionResult<ApiResponseDto<TransactionResponseDto>>> CreateTransaction(
            CreateTransactionRequestDto request,
            [FromQuery] int userId)
        {
            try
            {
                // Validate category exists
                var category = await _context.Categories.FindAsync(request.CategoryId);
                if (category == null)
                {
                    return BadRequest(ApiResponseDto<TransactionResponseDto>.ErrorResponse("Category not found"));
                }

                using var transaction = await _context.Database.BeginTransactionAsync();

                // Create transaction
                var newTransaction = new Transaction
                {
                    UserId = userId,
                    CategoryId = request.CategoryId,
                    TransactionType = request.TransactionType,
                    Amount = request.Amount,
                    Description = request.Description,
                    TransactionDate = request.TransactionDate.Date,
                    IsRecurring = request.IsRecurring,
                    RecurringFrequency = request.RecurringFrequency,
                    RecurringEndDate = request.RecurringEndDate,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Transactions.Add(newTransaction);
                await _context.SaveChangesAsync();

                // Handle budget impact for expense transactions
                var budgetImpacts = new List<BudgetImpactDto>();
                if (request.TransactionType.ToLower() == "expense")
                {
                    budgetImpacts = await _budgetService.ProcessBudgetImpactAsync(
                        userId,
                        request.CategoryId,
                        request.Amount,
                        request.TransactionDate,
                        newTransaction.TransactionId);
                }

                await transaction.CommitAsync();

                // Return created transaction
                var transactionDto = new TransactionResponseDto
                {
                    TransactionId = newTransaction.TransactionId,
                    UserId = newTransaction.UserId,
                    CategoryId = newTransaction.CategoryId,
                    CategoryName = category.CategoryName,
                    TransactionType = newTransaction.TransactionType,
                    Amount = newTransaction.Amount,
                    Description = newTransaction.Description,
                    TransactionDate = newTransaction.TransactionDate,
                    IsRecurring = newTransaction.IsRecurring,
                    RecurringFrequency = newTransaction.RecurringFrequency,
                    RecurringEndDate = newTransaction.RecurringEndDate,
                    CreatedAt = newTransaction.CreatedAt,
                    UpdatedAt = newTransaction.UpdatedAt,
                    BudgetImpacts = budgetImpacts
                };

                return CreatedAtAction(nameof(GetTransactionById),
                    new { transactionId = newTransaction.TransactionId },
                    ApiResponseDto<TransactionResponseDto>.SuccessResponse(transactionDto, "Transaction created successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating transaction for user {UserId}", userId);
                return StatusCode(500, ApiResponseDto<TransactionResponseDto>.ErrorResponse("An error occurred while creating the transaction"));
            }
        }

        // PUT: api/Transaction/{transactionId}
        [HttpPut("{transactionId}")]
        public async Task<ActionResult<ApiResponseDto<TransactionResponseDto>>> UpdateTransaction(
            int transactionId,
            CreateTransactionRequestDto request)
        {
            try
            {
                var existingTransaction = await _context.Transactions
                    .Include(t => t.Category)
                    .Include(t => t.TransactionBudgetImpacts)
                    .FirstOrDefaultAsync(t => t.TransactionId == transactionId);

                if (existingTransaction == null)
                {
                    return NotFound(ApiResponseDto<TransactionResponseDto>.ErrorResponse("Transaction not found"));
                }

                // Validate category exists
                var category = await _context.Categories.FindAsync(request.CategoryId);
                if (category == null)
                {
                    return BadRequest(ApiResponseDto<TransactionResponseDto>.ErrorResponse("Category not found"));
                }

                using var transaction = await _context.Database.BeginTransactionAsync();

                // Store old values for budget impact reversal
                var oldAmount = existingTransaction.Amount;
                var oldCategoryId = existingTransaction.CategoryId;
                var oldTransactionType = existingTransaction.TransactionType;
                var oldTransactionDate = existingTransaction.TransactionDate;

                // Update transaction
                existingTransaction.CategoryId = request.CategoryId;
                existingTransaction.TransactionType = request.TransactionType;
                existingTransaction.Amount = request.Amount;
                existingTransaction.Description = request.Description;
                existingTransaction.TransactionDate = request.TransactionDate.Date;
                existingTransaction.IsRecurring = request.IsRecurring;
                existingTransaction.RecurringFrequency = request.RecurringFrequency;
                existingTransaction.RecurringEndDate = request.RecurringEndDate;
                existingTransaction.UpdatedAt = DateTime.UtcNow;

                // Remove old budget impacts
                if (oldTransactionType.ToLower() == "expense")
                {
                    await _budgetService.ReverseBudgetImpactAsync(transactionId);
                }

                await _context.SaveChangesAsync();

                // Apply new budget impacts for expense transactions
                var budgetImpacts = new List<BudgetImpactDto>();
                if (request.TransactionType.ToLower() == "expense")
                {
                    budgetImpacts = await _budgetService.ProcessBudgetImpactAsync(
                        existingTransaction.UserId,
                        request.CategoryId,
                        request.Amount,
                        request.TransactionDate,
                        transactionId);
                }

                await transaction.CommitAsync();

                // Return updated transaction
                var transactionDto = new TransactionResponseDto
                {
                    TransactionId = existingTransaction.TransactionId,
                    UserId = existingTransaction.UserId,
                    CategoryId = existingTransaction.CategoryId,
                    CategoryName = category.CategoryName,
                    TransactionType = existingTransaction.TransactionType,
                    Amount = existingTransaction.Amount,
                    Description = existingTransaction.Description,
                    TransactionDate = existingTransaction.TransactionDate,
                    IsRecurring = existingTransaction.IsRecurring,
                    RecurringFrequency = existingTransaction.RecurringFrequency,
                    RecurringEndDate = existingTransaction.RecurringEndDate,
                    CreatedAt = existingTransaction.CreatedAt,
                    UpdatedAt = existingTransaction.UpdatedAt,
                    BudgetImpacts = budgetImpacts
                };

                return Ok(ApiResponseDto<TransactionResponseDto>.SuccessResponse(transactionDto, "Transaction updated successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating transaction {TransactionId}", transactionId);
                return StatusCode(500, ApiResponseDto<TransactionResponseDto>.ErrorResponse("An error occurred while updating the transaction"));
            }
        }

        // DELETE: api/Transaction/{transactionId}
        [HttpDelete("{transactionId}")]
        public async Task<ActionResult<ApiResponseDto<object>>> DeleteTransaction(int transactionId)
        {
            try
            {
                var transaction = await _context.Transactions
                    .Include(t => t.TransactionBudgetImpacts)
                    .FirstOrDefaultAsync(t => t.TransactionId == transactionId);

                if (transaction == null)
                {
                    return NotFound(ApiResponseDto<object>.ErrorResponse("Transaction not found"));
                }

                using var dbTransaction = await _context.Database.BeginTransactionAsync();

                // Reverse budget impacts for expense transactions
                if (transaction.TransactionType.ToLower() == "expense")
                {
                    await _budgetService.ReverseBudgetImpactAsync(transactionId);
                }

                // Remove transaction
                _context.Transactions.Remove(transaction);
                await _context.SaveChangesAsync();

                await dbTransaction.CommitAsync();

                return Ok(ApiResponseDto<object>.SuccessResponse(null, "Transaction deleted successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting transaction {TransactionId}", transactionId);
                return StatusCode(500, ApiResponseDto<object>.ErrorResponse("An error occurred while deleting the transaction"));
            }
        }

        // GET: api/Transaction/user/{userId}/summary
        [HttpGet("user/{userId}/summary")]
        public async Task<ActionResult<ApiResponseDto<object>>> GetTransactionSummary(
            int userId,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var query = _context.Transactions
                    .Where(t => t.UserId == userId);

                if (startDate.HasValue)
                    query = query.Where(t => t.TransactionDate >= startDate.Value.Date);

                if (endDate.HasValue)
                    query = query.Where(t => t.TransactionDate <= endDate.Value.Date);

                var summary = await query
                    .GroupBy(t => t.TransactionType)
                    .Select(g => new
                    {
                        TransactionType = g.Key,
                        TotalAmount = g.Sum(t => t.Amount),
                        Count = g.Count()
                    })
                    .ToListAsync();

                var totalIncome = summary.FirstOrDefault(s => s.TransactionType == "Income")?.TotalAmount ?? 0;
                var totalExpense = summary.FirstOrDefault(s => s.TransactionType == "Expense")?.TotalAmount ?? 0;

                var result = new
                {
                    TotalIncome = totalIncome,
                    TotalExpense = totalExpense,
                    NetAmount = totalIncome - totalExpense,
                    Summary = summary
                };

                return Ok(ApiResponseDto<object>.SuccessResponse(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving transaction summary for user {UserId}", userId);
                return StatusCode(500, ApiResponseDto<object>.ErrorResponse("An error occurred while retrieving transaction summary"));
            }
        }
    }
}
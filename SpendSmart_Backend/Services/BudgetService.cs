using Microsoft.EntityFrameworkCore;
using SpendSmart_Backend.Data;
using SpendSmart_Backend.Models;
using SpendSmart_Backend.DTOs;

namespace SpendSmart_Backend.Services
{
    public class BudgetService : IBudgetService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<BudgetService> _logger;

        public BudgetService(ApplicationDbContext context, ILogger<BudgetService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<BudgetImpactDto>> ProcessBudgetImpactAsync(
            int userId,
            int categoryId,
            decimal amount,
            DateTime transactionDate,
            int transactionId)
        {
            var budgetImpacts = new List<BudgetImpactDto>();

            try
            {
                // Find all active budgets that match the transaction date and user
                var applicableBudgets = await _context.Budgets
                    .Where(b => b.UserId == userId &&
                               b.Status == "Active" &&
                               b.StartDate <= transactionDate.Date &&
                               b.EndDate >= transactionDate.Date)
                    .Include(b => b.BudgetCategories.Where(bc => bc.CategoryId == categoryId))
                    .ToListAsync();

                foreach (var budget in applicableBudgets)
                {
                    var budgetCategory = budget.BudgetCategories.FirstOrDefault(bc => bc.CategoryId == categoryId);

                    // Only process if the budget has this category allocated
                    if (budgetCategory != null)
                    {
                        // Create transaction budget impact record
                        var transactionBudgetImpact = new TransactionBudgetImpact
                        {
                            TransactionId = transactionId,
                            BudgetId = budget.BudgetId,
                            CategoryId = categoryId,
                            ImpactAmount = amount,
                            CreatedAt = DateTime.UtcNow
                        };

                        _context.TransactionBudgetImpacts.Add(transactionBudgetImpact);

                        // Update budget category spent amount
                        budgetCategory.SpentAmount += amount;
                        budgetCategory.UpdatedAt = DateTime.UtcNow;

                        // Update total budget spent amount
                        budget.TotalSpentAmount += amount;
                        budget.UpdatedAt = DateTime.UtcNow;

                        budgetImpacts.Add(new BudgetImpactDto
                        {
                            BudgetId = budget.BudgetId,
                            BudgetName = budget.BudgetName,
                            ImpactAmount = amount
                        });

                        _logger.LogInformation("Applied budget impact: Budget {BudgetId}, Category {CategoryId}, Amount {Amount}",
                            budget.BudgetId, categoryId, amount);
                    }
                }

                await _context.SaveChangesAsync();
                return budgetImpacts;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing budget impact for transaction {TransactionId}", transactionId);
                throw;
            }
        }

        public async Task ReverseBudgetImpactAsync(int transactionId)
        {
            try
            {
                var budgetImpacts = await _context.TransactionBudgetImpacts
                    .Where(tbi => tbi.TransactionId == transactionId)
                    .Include(tbi => tbi.Budget)
                    .ThenInclude(b => b.BudgetCategories)
                    .ToListAsync();

                foreach (var impact in budgetImpacts)
                {
                    // Find the budget category that was impacted
                    var budgetCategory = impact.Budget.BudgetCategories
                        .FirstOrDefault(bc => bc.CategoryId == impact.CategoryId);

                    if (budgetCategory != null)
                    {
                        // Reverse the spent amount in budget category
                        budgetCategory.SpentAmount -= impact.ImpactAmount;
                        budgetCategory.UpdatedAt = DateTime.UtcNow;

                        // Ensure spent amount doesn't go below zero
                        if (budgetCategory.SpentAmount < 0)
                            budgetCategory.SpentAmount = 0;
                    }

                    // Reverse the total spent amount in budget
                    impact.Budget.TotalSpentAmount -= impact.ImpactAmount;
                    impact.Budget.UpdatedAt = DateTime.UtcNow;

                    // Ensure total spent amount doesn't go below zero
                    if (impact.Budget.TotalSpentAmount < 0)
                        impact.Budget.TotalSpentAmount = 0;

                    _logger.LogInformation("Reversed budget impact: Budget {BudgetId}, Category {CategoryId}, Amount {Amount}",
                        impact.BudgetId, impact.CategoryId, impact.ImpactAmount);
                }

                // Remove the impact records
                _context.TransactionBudgetImpacts.RemoveRange(budgetImpacts);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reversing budget impact for transaction {TransactionId}", transactionId);
                throw;
            }
        }

        public async Task RecalculateBudgetAmountsAsync(int budgetId)
        {
            try
            {
                var budget = await _context.Budgets
                    .Include(b => b.BudgetCategories)
                    .Include(b => b.TransactionBudgetImpacts)
                    .FirstOrDefaultAsync(b => b.BudgetId == budgetId);

                if (budget == null)
                {
                    _logger.LogWarning("Budget {BudgetId} not found for recalculation", budgetId);
                    return;
                }

                // Recalculate total spent amount from transaction impacts
                budget.TotalSpentAmount = budget.TransactionBudgetImpacts.Sum(tbi => tbi.ImpactAmount);

                // Recalculate spent amounts for each category
                foreach (var budgetCategory in budget.BudgetCategories)
                {
                    budgetCategory.SpentAmount = budget.TransactionBudgetImpacts
                        .Where(tbi => tbi.CategoryId == budgetCategory.CategoryId)
                        .Sum(tbi => tbi.ImpactAmount);

                    budgetCategory.UpdatedAt = DateTime.UtcNow;
                }

                budget.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Recalculated budget amounts for budget {BudgetId}", budgetId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recalculating budget amounts for budget {BudgetId}", budgetId);
                throw;
            }
        }

        public async Task<bool> IsBudgetOverLimitAsync(int budgetId, int categoryId)
        {
            try
            {
                var budgetCategory = await _context.BudgetCategories
                    .FirstOrDefaultAsync(bc => bc.BudgetId == budgetId && bc.CategoryId == categoryId);

                if (budgetCategory == null)
                    return false;

                return budgetCategory.SpentAmount > budgetCategory.AllocatedAmount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking budget limit for budget {BudgetId}, category {CategoryId}", budgetId, categoryId);
                return false;
            }
        }
    }
}

using Microsoft.EntityFrameworkCore;
using SpendSmart_Backend.Data;
using SpendSmart_Backend.DTOs;
using SpendSmart_Backend.Models;

namespace SpendSmart_Backend.Services
{
    public class BudgetService
    {
        private readonly ApplicationDbContext _context;

        public BudgetService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<BudgetResponseDto?> GetBudgetDetailsAsync(int budgetId)
        {
            var budget = await _context.Budgets
                .Include(b => b.BudgetCategories)
                .ThenInclude(bc => bc.Category)
                .FirstOrDefaultAsync(b => b.BudgetId == budgetId);

            if (budget == null)
                return null;

            // Calculate days remaining
            int daysRemaining = (budget.EndDate - DateTime.Today).Days;
            if (daysRemaining < 0) daysRemaining = 0;

            // Calculate progress percentage
            decimal progressPercentage = 0;
            if (budget.TotalBudgetAmount > 0)
            {
                progressPercentage = (budget.TotalSpentAmount / budget.TotalBudgetAmount) * 100;
                progressPercentage = Math.Min(progressPercentage, 100); // Cap at 100%
            }

            // Map budget categories to DTOs
            var categoryDtos = budget.BudgetCategories.Select(bc => new BudgetCategoryResponseDto
            {
                CategoryId = bc.CategoryId,
                CategoryName = bc.Category.CategoryName,
                CategoryIcon = bc.Category.Icon,
                CategoryColor = bc.Category.Color,
                AllocatedAmount = bc.AllocatedAmount,
                SpentAmount = bc.SpentAmount,
                RemainingAmount = bc.AllocatedAmount - bc.SpentAmount,
                ProgressPercentage = bc.AllocatedAmount > 0 ? Math.Min((bc.SpentAmount / bc.AllocatedAmount) * 100, 100) : 0
            }).ToList();

            // Create the budget response DTO
            return new BudgetResponseDto
            {
                BudgetId = budget.BudgetId,
                BudgetName = budget.BudgetName,
                BudgetType = budget.BudgetType,
                StartDate = budget.StartDate,
                EndDate = budget.EndDate,
                TotalBudgetAmount = budget.TotalBudgetAmount,
                TotalSpentAmount = budget.TotalSpentAmount,
                RemainingAmount = budget.TotalBudgetAmount - budget.TotalSpentAmount,
                Description = budget.Description,
                ProgressPercentage = progressPercentage,
                DaysRemaining = daysRemaining,
                Status = budget.Status,
                Categories = categoryDtos
            };
        }

        public async Task<List<BudgetSummaryDto>> GetUserBudgetsAsync(int userId)
        {
            var budgets = await _context.Budgets
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.StartDate)
                .Select(b => new BudgetSummaryDto
                {
                    BudgetId = b.BudgetId,
                    BudgetName = b.BudgetName,
                    BudgetType = b.BudgetType,
                    StartDate = b.StartDate,
                    EndDate = b.EndDate,
                    TotalBudgetAmount = b.TotalBudgetAmount,
                    TotalSpentAmount = b.TotalSpentAmount,
                    RemainingAmount = b.TotalBudgetAmount - b.TotalSpentAmount,
                    ProgressPercentage = b.TotalBudgetAmount > 0 ? Math.Min((b.TotalSpentAmount / b.TotalBudgetAmount) * 100, 100) : 0,
                    Status = b.Status
                })
                .ToListAsync();

            return budgets;
        }

        public async Task<List<TransactionDetailsDto>> GetBudgetTransactionsAsync(int budgetId)
        {
            var budget = await _context.Budgets.FindAsync(budgetId);
            if (budget == null)
                return new List<TransactionDetailsDto>();

            // Get transactions that fall within the budget period and belong to the same user
            // This provides a more realistic view of expense transactions related to the budget
            var transactions = await _context.Transactions
                .Where(t => t.UserId == budget.UserId && 
                           t.TransactionType == "Expense" &&
                           t.TransactionDate >= budget.StartDate && 
                           t.TransactionDate <= budget.EndDate)
                .Include(t => t.Category)
                .OrderByDescending(t => t.TransactionDate)
                .Select(t => new TransactionDetailsDto
                {
                    TransactionId = t.TransactionId,
                    TransactionType = t.TransactionType,
                    CategoryId = t.CategoryId,
                    CategoryName = t.Category.CategoryName,
                    Amount = t.Amount,
                    TransactionDate = t.TransactionDate.ToString("yyyy-MM-dd"),
                    Description = t.Description,
                    MerchantName = t.MerchantName,
                    Location = t.Location,
                    Tags = t.Tags != null ? t.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries) : Array.Empty<string>(),
                    ReceiptUrl = t.ReceiptUrl,
                    IsRecurring = t.IsRecurring,
                    RecurringFrequency = t.RecurringFrequency,
                    RecurringEndDate = t.RecurringEndDate.HasValue ? t.RecurringEndDate.Value.ToString("yyyy-MM-dd") : null,
                    BudgetImpacts = new List<BudgetImpactDto>
                    {
                        new BudgetImpactDto
                        {
                            BudgetId = budgetId,
                            BudgetName = budget.BudgetName,
                            CategoryId = t.CategoryId,
                            CategoryName = t.Category.CategoryName,
                            ImpactAmount = t.Amount,
                            ImpactType = "Deduction"
                        }
                    }
                })
                .ToListAsync();

            return transactions;
        }

        public async Task<List<ExpenseBreakdownDto>> GetExpenseBreakdownAsync(int budgetId)
        {
            var budget = await _context.Budgets
                .Include(b => b.BudgetCategories)
                .ThenInclude(bc => bc.Category)
                .FirstOrDefaultAsync(b => b.BudgetId == budgetId);

            if (budget == null)
                return new List<ExpenseBreakdownDto>();

            var totalSpent = budget.TotalSpentAmount;
            
            var breakdown = budget.BudgetCategories
                .Where(bc => bc.SpentAmount > 0)
                .Select(bc => new ExpenseBreakdownDto
                {
                    CategoryId = bc.CategoryId,
                    CategoryName = bc.Category.CategoryName,
                    Amount = bc.SpentAmount,
                    Percentage = totalSpent > 0 ? (bc.SpentAmount / totalSpent) * 100 : 0,
                    Color = bc.Category.Color,
                    Icon = bc.Category.Icon
                })
                .OrderByDescending(eb => eb.Amount)
                .ToList();

            return breakdown;
        }

        public async Task<List<PeriodDataDto>> GetBudgetPeriodDataAsync(int budgetId)
        {
            var budget = await _context.Budgets
                .Include(b => b.TransactionBudgetImpacts)
                .ThenInclude(tbi => tbi.Transaction)
                .FirstOrDefaultAsync(b => b.BudgetId == budgetId);

            if (budget == null)
                return new List<PeriodDataDto>();

            // Get all transaction dates within the budget period
            var transactions = budget.TransactionBudgetImpacts
                .Select(tbi => new
                {
                    Date = tbi.Transaction.TransactionDate.Date,
                    Amount = tbi.ImpactAmount
                })
                .OrderBy(t => t.Date)
                .ToList();

            // Group transactions by date and calculate daily totals
            var dailyTotals = transactions
                .GroupBy(t => t.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Amount = g.Sum(t => t.Amount)
                })
                .OrderBy(d => d.Date)
                .ToList();

            // Calculate budget limit per day
            decimal dailyBudgetLimit = 0;
            int totalDays = (budget.EndDate - budget.StartDate).Days + 1;
            if (totalDays > 0)
            {
                dailyBudgetLimit = budget.TotalBudgetAmount / totalDays;
            }

            // Generate period data for each day in the budget period
            var periodData = new List<PeriodDataDto>();
            decimal cumulativeAmount = 0;

            for (var date = budget.StartDate; date <= budget.EndDate; date = date.AddDays(1))
            {
                var dailyTotal = dailyTotals.FirstOrDefault(d => d.Date == date.Date)?.Amount ?? 0;
                cumulativeAmount += dailyTotal;

                periodData.Add(new PeriodDataDto
                {
                    Date = date,
                    Amount = dailyTotal,
                    CumulativeAmount = cumulativeAmount,
                    BudgetLimit = dailyBudgetLimit * (date - budget.StartDate).Days
                });
            }

            return periodData;
        }

        public async Task<BudgetResponseDto> CreateBudgetAsync(int userId, CreateBudgetDto createBudgetDto)
        {
            // Check if user exists
            var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
            if (!userExists)
            {
                throw new ArgumentException($"User with ID {userId} does not exist.");
            }
            
            // Calculate end date based on budget type
            DateTime endDate;
            if (createBudgetDto.BudgetType.ToLower() == "monthly")
            {
                endDate = createBudgetDto.StartDate.AddMonths(1).AddDays(-1);
            }
            else if (createBudgetDto.BudgetType.ToLower() == "annually")
            {
                endDate = createBudgetDto.StartDate.AddYears(1).AddDays(-1);
            }
            else
            {
                throw new ArgumentException("Invalid budget type. Must be 'Monthly' or 'Annually'.");
            }

            // Calculate total budget amount
            decimal totalBudgetAmount = createBudgetDto.CategoryAllocations.Sum(ca => ca.AllocatedAmount);

            // Create new budget
            var budget = new Budget
            {
                UserId = userId,
                BudgetName = createBudgetDto.BudgetName,
                BudgetType = createBudgetDto.BudgetType,
                StartDate = createBudgetDto.StartDate,
                EndDate = endDate,
                TotalBudgetAmount = totalBudgetAmount,
                Description = createBudgetDto.Description,
                Status = "Active",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Budgets.Add(budget);
            await _context.SaveChangesAsync();

            // Create budget categories
            foreach (var allocation in createBudgetDto.CategoryAllocations)
            {
                var budgetCategory = new BudgetCategory
                {
                    BudgetId = budget.BudgetId,
                    CategoryId = allocation.CategoryId,
                    AllocatedAmount = allocation.AllocatedAmount,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.BudgetCategories.Add(budgetCategory);
            }

            await _context.SaveChangesAsync();

            // Return the newly created budget details
            return await GetBudgetDetailsAsync(budget.BudgetId);
        }

        public async Task<BudgetResponseDto?> UpdateBudgetAsync(int budgetId, UpdateBudgetDto updateBudgetDto)
        {
            var budget = await _context.Budgets
                .Include(b => b.BudgetCategories)
                .FirstOrDefaultAsync(b => b.BudgetId == budgetId);

            if (budget == null)
                return null;

            // Calculate end date based on budget type
            DateTime endDate;
            if (updateBudgetDto.BudgetType.ToLower() == "monthly")
            {
                endDate = updateBudgetDto.StartDate.AddMonths(1).AddDays(-1);
            }
            else if (updateBudgetDto.BudgetType.ToLower() == "annually")
            {
                endDate = updateBudgetDto.StartDate.AddYears(1).AddDays(-1);
            }
            else
            {
                throw new ArgumentException("Invalid budget type. Must be 'Monthly' or 'Annually'.");
            }

            // Update budget properties
            budget.BudgetName = updateBudgetDto.BudgetName;
            budget.BudgetType = updateBudgetDto.BudgetType;
            budget.StartDate = updateBudgetDto.StartDate;
            budget.EndDate = endDate;
            budget.Description = updateBudgetDto.Description;
            budget.UpdatedAt = DateTime.UtcNow;

            // Calculate new total budget amount
            decimal totalBudgetAmount = updateBudgetDto.CategoryAllocations.Sum(ca => ca.AllocatedAmount);
            budget.TotalBudgetAmount = totalBudgetAmount;

            // Update or create budget categories
            foreach (var allocation in updateBudgetDto.CategoryAllocations)
            {
                var existingCategory = budget.BudgetCategories.FirstOrDefault(bc => bc.CategoryId == allocation.CategoryId);

                if (existingCategory != null)
                {
                    // Update existing category allocation
                    existingCategory.AllocatedAmount = allocation.AllocatedAmount;
                    existingCategory.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    // Create new category allocation
                    var budgetCategory = new BudgetCategory
                    {
                        BudgetId = budget.BudgetId,
                        CategoryId = allocation.CategoryId,
                        AllocatedAmount = allocation.AllocatedAmount,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _context.BudgetCategories.Add(budgetCategory);
                }
            }

            // Remove categories that are no longer in the allocation list
            var categoriesToRemove = budget.BudgetCategories
                .Where(bc => !updateBudgetDto.CategoryAllocations.Any(ca => ca.CategoryId == bc.CategoryId))
                .ToList();

            foreach (var category in categoriesToRemove)
            {
                _context.BudgetCategories.Remove(category);
            }

            await _context.SaveChangesAsync();

            // Return the updated budget details
            return await GetBudgetDetailsAsync(budget.BudgetId);
        }

        public async Task<bool> DeleteBudgetAsync(int budgetId)
        {
            var budget = await _context.Budgets.FindAsync(budgetId);
            if (budget == null)
                return false;

            _context.Budgets.Remove(budget);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateBudgetStatusAsync(int budgetId, string status)
        {
            var budget = await _context.Budgets.FindAsync(budgetId);
            if (budget == null)
                return false;

            if (status != "Active" && status != "Completed" && status != "Cancelled")
                return false;

            budget.Status = status;
            budget.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task UpdateBudgetAmountsAsync(int budgetId)
        {
            var budget = await _context.Budgets
                .Include(b => b.BudgetCategories)
                .Include(b => b.TransactionBudgetImpacts)
                .FirstOrDefaultAsync(b => b.BudgetId == budgetId);

            if (budget == null)
                return;

            // Calculate total spent amount from all transaction impacts
            decimal totalSpent = budget.TransactionBudgetImpacts.Sum(tbi => tbi.ImpactAmount);
            budget.TotalSpentAmount = totalSpent;

            // Update spent amounts for each category
            foreach (var category in budget.BudgetCategories)
            {
                decimal categorySpent = budget.TransactionBudgetImpacts
                    .Where(tbi => tbi.CategoryId == category.CategoryId)
                    .Sum(tbi => tbi.ImpactAmount);

                category.SpentAmount = categorySpent;
                category.UpdatedAt = DateTime.UtcNow;
            }

            // Update budget status if needed
            if (budget.Status == "Active")
            {
                if (DateTime.Today > budget.EndDate)
                {
                    budget.Status = "Completed";
                }
                else if (budget.TotalSpentAmount >= budget.TotalBudgetAmount)
                {
                    budget.Status = "Exceeded";
                }
            }

            budget.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        public async Task RecordTransactionImpactAsync(int transactionId, int budgetId, int categoryId, decimal amount)
        {
            var impact = new TransactionBudgetImpact
            {
                TransactionId = transactionId,
                BudgetId = budgetId,
                CategoryId = categoryId,
                ImpactAmount = amount,
                CreatedAt = DateTime.UtcNow
            };

            _context.TransactionBudgetImpacts.Add(impact);
            await _context.SaveChangesAsync();

            // Update budget amounts
            await UpdateBudgetAmountsAsync(budgetId);
        }
    }
} 
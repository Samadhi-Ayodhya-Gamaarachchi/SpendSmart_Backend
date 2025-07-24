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
                .Include(b => b.TransactionBudgetImpacts)
                .FirstOrDefaultAsync(b => b.BudgetId == budgetId);

            if (budget == null)
                return null;

            // Calculate total spent amount from transaction impacts (like backend folder)
            decimal totalActualSpent = budget.TransactionBudgetImpacts.Sum(tbi => tbi.ImpactAmount);

            // Calculate days remaining
            int daysRemaining = (budget.EndDate - DateTime.Today).Days;
            if (daysRemaining < 0) daysRemaining = 0;

            // Calculate progress percentage based on actual spending
            decimal progressPercentage = 0;
            if (budget.TotalBudgetAmount > 0)
            {
                progressPercentage = (totalActualSpent / budget.TotalBudgetAmount) * 100;
                progressPercentage = Math.Min(progressPercentage, 100); // Cap at 100%
            }

            // Calculate spent amounts per category from transaction impacts
            var categorySpending = budget.TransactionBudgetImpacts
                .GroupBy(tbi => tbi.CategoryId)
                .ToDictionary(g => g.Key, g => g.Sum(tbi => tbi.ImpactAmount));

            // Map budget categories to DTOs with actual spending from impacts
            var categoryDtos = budget.BudgetCategories.Select(bc => new BudgetCategoryResponseDto
            {
                CategoryId = bc.CategoryId,
                CategoryName = bc.Category.CategoryName,
                CategoryIcon = bc.Category.Icon,
                CategoryColor = bc.Category.Color,
                AllocatedAmount = bc.AllocatedAmount,
                SpentAmount = categorySpending.GetValueOrDefault(bc.CategoryId, 0),
                RemainingAmount = bc.AllocatedAmount - categorySpending.GetValueOrDefault(bc.CategoryId, 0),
                ProgressPercentage = bc.AllocatedAmount > 0 ? Math.Round((categorySpending.GetValueOrDefault(bc.CategoryId, 0) / bc.AllocatedAmount) * 100 * 10) / 10 : 0
            }).ToList();

            // Determine status based on actual spending
            var actualStatus = budget.Status;
            if (actualStatus == "Active")
            {
                if (DateTime.Today > budget.EndDate)
                {
                    actualStatus = "Completed";
                }
                else if (totalActualSpent >= budget.TotalBudgetAmount)
                {
                    actualStatus = "Exceeded";
                }
            }

            // Create the budget response DTO
            return new BudgetResponseDto
            {
                BudgetId = budget.BudgetId,
                BudgetName = budget.BudgetName,
                BudgetType = budget.BudgetType,
                StartDate = budget.StartDate,
                EndDate = budget.EndDate,
                TotalBudgetAmount = budget.TotalBudgetAmount,
                TotalSpentAmount = totalActualSpent,
                RemainingAmount = budget.TotalBudgetAmount - totalActualSpent,
                Description = budget.Description,
                ProgressPercentage = Math.Round(progressPercentage * 10) / 10, // Round to 1 decimal place
                DaysRemaining = daysRemaining,
                Status = actualStatus,
                Categories = categoryDtos
            };
        }

        public async Task<List<BudgetSummaryDto>> GetUserBudgetsAsync(int userId)
        {
            var budgets = await _context.Budgets
                .Include(b => b.TransactionBudgetImpacts)
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.StartDate)
                .ToListAsync();

            var budgetSummaries = budgets.Select(b => {
                // Calculate total spent amount from transaction impacts (consistent with GetBudgetDetailsAsync)
                decimal totalActualSpent = b.TransactionBudgetImpacts.Sum(tbi => tbi.ImpactAmount);
                
                // Calculate progress percentage based on actual spending (consistent with GetBudgetDetailsAsync)
                decimal progressPercentage = 0;
                if (b.TotalBudgetAmount > 0)
                {
                    progressPercentage = (totalActualSpent / b.TotalBudgetAmount) * 100;
                    progressPercentage = Math.Min(progressPercentage, 100); // Cap at 100%
                }

                return new BudgetSummaryDto
                {
                    BudgetId = b.BudgetId,
                    BudgetName = b.BudgetName,
                    BudgetType = b.BudgetType,
                    StartDate = b.StartDate,
                    EndDate = b.EndDate,
                    TotalBudgetAmount = b.TotalBudgetAmount,
                    TotalSpentAmount = totalActualSpent, // Use actual spent from impacts
                    RemainingAmount = b.TotalBudgetAmount - totalActualSpent,
                    ProgressPercentage = Math.Round(progressPercentage * 10) / 10, // Round to 1 decimal place, same as GetBudgetDetailsAsync
                    Status = b.Status
                };
            }).ToList();

            return budgetSummaries;
        }

        public async Task<List<TransactionDetailsDto>> GetBudgetTransactionsAsync(int budgetId)
        {
            try
            {
                // Get budget with name for budget impact
                var budget = await _context.Budgets.FindAsync(budgetId);
                if (budget == null)
                    return new List<TransactionDetailsDto>();

                // Use TransactionBudgetImpacts to get transactions related to this budget (like backend folder)
                var transactionImpacts = await _context.TransactionBudgetImpacts
                    .Where(tbi => tbi.BudgetId == budgetId)
                    .Include(tbi => tbi.Transaction)
                    .ThenInclude(t => t.Category)
                    .ToListAsync();

                var transactions = transactionImpacts.Select(tbi => new TransactionDetailsDto
                {
                    TransactionId = tbi.TransactionId,
                    TransactionType = tbi.Transaction?.TransactionType ?? "Expense",
                    CategoryId = tbi.Transaction?.CategoryId ?? tbi.CategoryId,
                    CategoryName = tbi.Transaction?.Category?.CategoryName ?? "Unknown",
                    Amount = tbi.Transaction?.Amount ?? tbi.ImpactAmount,
                    TransactionDate = tbi.Transaction?.TransactionDate.ToString("yyyy-MM-dd") ?? DateTime.Now.ToString("yyyy-MM-dd"),
                    Description = tbi.Transaction?.Description ?? "",
                    MerchantName = tbi.Transaction?.MerchantName ?? "",
                    Location = tbi.Transaction?.Location ?? "",
                    Tags = tbi.Transaction?.Tags != null ? tbi.Transaction.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries) : Array.Empty<string>(),
                    ReceiptUrl = tbi.Transaction?.ReceiptUrl ?? "",
                    IsRecurring = tbi.Transaction?.IsRecurring ?? false,
                    RecurringFrequency = tbi.Transaction?.RecurringFrequency ?? "",
                    RecurringEndDate = tbi.Transaction?.RecurringEndDate?.ToString("yyyy-MM-dd"),
                    CategoryIcon = tbi.Transaction?.Category?.Icon ?? "💰",
                    CategoryColor = tbi.Transaction?.Category?.Color ?? "#666666",
                    BudgetImpacts = new List<BudgetImpactDto>
                    {
                        new BudgetImpactDto
                        {
                            BudgetId = tbi.BudgetId,
                            BudgetName = budget.BudgetName ?? "Unknown Budget",
                            CategoryId = tbi.CategoryId,
                            CategoryName = tbi.Transaction?.Category?.CategoryName ?? "Unknown",
                            ImpactAmount = tbi.ImpactAmount,
                            ImpactType = "Deduction"
                        }
                    }
                })
                .OrderByDescending(t => t.TransactionDate)
                .ToList();

                return transactions;
            }
            catch (Exception ex)
            {
                // Log the exception (in a real application, use proper logging)
                Console.WriteLine($"Error in GetBudgetTransactionsAsync: {ex.Message}");
                return new List<TransactionDetailsDto>();
            }
        }

        public async Task<List<ExpenseBreakdownDto>> GetExpenseBreakdownAsync(int budgetId)
        {
            var budget = await _context.Budgets
                .Include(b => b.BudgetCategories)
                .ThenInclude(bc => bc.Category)
                .Include(b => b.TransactionBudgetImpacts)
                .FirstOrDefaultAsync(b => b.BudgetId == budgetId);

            if (budget == null)
                return new List<ExpenseBreakdownDto>();

            // Calculate total spent from transaction impacts (like backend folder)
            var totalSpent = budget.TransactionBudgetImpacts.Sum(tbi => tbi.ImpactAmount);

            // Group impacts by category to get category spending
            var categorySpending = budget.TransactionBudgetImpacts
                .GroupBy(tbi => tbi.CategoryId)
                .ToDictionary(g => g.Key, g => g.Sum(tbi => tbi.ImpactAmount));

            var breakdown = budget.BudgetCategories
                .Where(bc => categorySpending.ContainsKey(bc.CategoryId) && categorySpending[bc.CategoryId] > 0)
                .Select(bc => new ExpenseBreakdownDto
                {
                    CategoryId = bc.CategoryId,
                    CategoryName = bc.Category.CategoryName,
                    Amount = categorySpending[bc.CategoryId],
                    Percentage = totalSpent > 0 ? Math.Round((categorySpending[bc.CategoryId] / totalSpent) * 100 * 10) / 10 : 0,
                    Color = bc.Category.Color ?? "#666666",
                    Icon = bc.Category.Icon ?? "💰"
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
            var budgetDetails = await GetBudgetDetailsAsync(budget.BudgetId);
            return budgetDetails ?? throw new InvalidOperationException("Failed to retrieve created budget details");
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

        // New method to populate missing budget impacts for existing transactions
        public async Task PopulateMissingBudgetImpactsAsync()
        {
            // Get all expense transactions that don't have budget impacts
            var expenseTransactionsWithoutImpacts = await _context.Transactions
                .Where(t => t.TransactionType == "Expense" &&
                           !_context.TransactionBudgetImpacts.Any(tbi => tbi.TransactionId == t.TransactionId))
                .Include(t => t.Category)
                .ToListAsync();

            foreach (var transaction in expenseTransactionsWithoutImpacts)
            {
                // Find active budgets that include this category and date range
                var activeBudgets = await _context.Budgets
                    .Include(b => b.BudgetCategories)
                    .Where(b =>
                        b.UserId == transaction.UserId &&
                        b.BudgetCategories.Any(bc => bc.CategoryId == transaction.CategoryId) &&
                        b.StartDate <= transaction.TransactionDate &&
                        b.EndDate >= transaction.TransactionDate)
                    .ToListAsync();

                foreach (var budget in activeBudgets)
                {
                    // Record impact on this budget
                    await RecordTransactionImpactAsync(
                        transaction.TransactionId,
                        budget.BudgetId,
                        transaction.CategoryId,
                        transaction.Amount
                    );
                }
            }
        }
    }
}
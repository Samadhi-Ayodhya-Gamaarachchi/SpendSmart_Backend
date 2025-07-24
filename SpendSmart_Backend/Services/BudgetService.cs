using Microsoft.EntityFrameworkCore;
using SpendSmart_Backend.Data;
using SpendSmart_Backend.DTOs;
using SpendSmart_Backend.Models;

namespace SpendSmart_Backend.Services
{
    public class BudgetService : IBudgetService
    {
        private readonly ApplicationDbContext _context;

        public BudgetService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<BudgetSummaryDto> CreateBudgetAsync(int userId, CreateBudgetDto dto)
        {
            // Check if budget already exists for this user, category, and month
            var existingBudget = await _context.Budgets
                .FirstOrDefaultAsync(b => b.UserId == userId && 
                                         b.CategoryId == dto.CategoryId && 
                                         b.MonthYear.Year == dto.MonthYear.Year && 
                                         b.MonthYear.Month == dto.MonthYear.Month);

            if (existingBudget != null)
            {
                throw new ArgumentException("Budget already exists for this category in the specified month");
            }

            // Validate category exists
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == dto.CategoryId);

            if (category == null)
            {
                throw new ArgumentException("Category not found");
            }

            var budget = new Budget
            {
                MonthYear = new DateTime(dto.MonthYear.Year, dto.MonthYear.Month, 1),
                AllocatedAmount = dto.AllocatedAmount,
                SpendAmount = dto.SpendAmount,
                Description = dto.Description,
                UserId = userId,
                CategoryId = dto.CategoryId
            };

            _context.Budgets.Add(budget);
            await _context.SaveChangesAsync();

            return await GetBudgetSummaryByIdAsync(userId, budget.Id);
        }

        public async Task<BudgetSummaryDto> GetBudgetSummaryByIdAsync(int userId, int id)
        {
            var budget = await _context.Budgets
                .Include(b => b.Category)
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

            if (budget == null)
            {
                throw new ArgumentException("Budget not found");
            }

            return CreateBudgetSummaryDto(budget);
        }

        public async Task<List<BudgetListDto>> GetUserBudgetsAsync(int userId)
        {
            var budgets = await _context.Budgets
                .Include(b => b.Category)
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.MonthYear)
                .ThenBy(b => b.Category.Name)
                .ToListAsync();

            return budgets.Select(b => CreateBudgetListDto(b)).ToList();
        }

        public async Task<BudgetOverviewDto> GetBudgetOverviewAsync(int userId, DateTime monthYear)
        {
            var targetDate = new DateTime(monthYear.Year, monthYear.Month, 1);
            
            var budgets = await _context.Budgets
                .Include(b => b.Category)
                .Where(b => b.UserId == userId && 
                           b.MonthYear.Year == targetDate.Year && 
                           b.MonthYear.Month == targetDate.Month)
                .ToListAsync();

            if (!budgets.Any())
            {
                return new BudgetOverviewDto
                {
                    MonthYear = targetDate,
                    TotalAllocated = 0,
                    TotalSpent = 0,
                    TotalRemaining = 0,
                    OverallSpendPercentage = 0,
                    TotalBudgets = 0,
                    OverBudgetCount = 0,
                    BudgetDetails = new List<BudgetListDto>()
                };
            }

            var totalAllocated = budgets.Sum(b => b.AllocatedAmount);
            var totalSpent = budgets.Sum(b => b.SpendAmount);
            var totalRemaining = totalAllocated - totalSpent;
            var overallSpendPercentage = totalAllocated > 0 ? (totalSpent / totalAllocated) * 100 : 0;
            var overBudgetCount = budgets.Count(b => b.SpendAmount > b.AllocatedAmount);

            return new BudgetOverviewDto
            {
                MonthYear = targetDate,
                TotalAllocated = totalAllocated,
                TotalSpent = totalSpent,
                TotalRemaining = totalRemaining,
                OverallSpendPercentage = Math.Round(overallSpendPercentage, 2),
                TotalBudgets = budgets.Count,
                OverBudgetCount = overBudgetCount,
                BudgetDetails = budgets.Select(b => CreateBudgetListDto(b)).ToList()
            };
        }

        public async Task<List<BudgetListDto>> GetBudgetsByMonthAsync(int userId, DateTime monthYear)
        {
            var targetDate = new DateTime(monthYear.Year, monthYear.Month, 1);
            
            var budgets = await _context.Budgets
                .Include(b => b.Category)
                .Where(b => b.UserId == userId && 
                           b.MonthYear.Year == targetDate.Year && 
                           b.MonthYear.Month == targetDate.Month)
                .OrderBy(b => b.Category.Name)
                .ToListAsync();

            return budgets.Select(b => CreateBudgetListDto(b)).ToList();
        }

        public async Task<BudgetSummaryDto> UpdateBudgetAsync(int userId, int id, UpdateBudgetDto dto)
        {
            var budget = await _context.Budgets
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

            if (budget == null)
            {
                throw new ArgumentException("Budget not found");
            }

            // Update fields if provided
            if (dto.MonthYear.HasValue)
            {
                budget.MonthYear = new DateTime(dto.MonthYear.Value.Year, dto.MonthYear.Value.Month, 1);
            }

            if (dto.AllocatedAmount.HasValue)
            {
                budget.AllocatedAmount = dto.AllocatedAmount.Value;
            }

            if (dto.SpendAmount.HasValue)
            {
                budget.SpendAmount = dto.SpendAmount.Value;
            }

            if (dto.Description != null)
            {
                budget.Description = dto.Description;
            }

            if (dto.CategoryId.HasValue)
            {
                // Validate category exists
                var category = await _context.Categories
                    .FirstOrDefaultAsync(c => c.Id == dto.CategoryId.Value);

                if (category == null)
                {
                    throw new ArgumentException("Category not found");
                }

                budget.CategoryId = dto.CategoryId.Value;
            }

            await _context.SaveChangesAsync();

            return await GetBudgetSummaryByIdAsync(userId, id);
        }

        public async Task<bool> DeleteBudgetAsync(int userId, int id)
        {
            var budget = await _context.Budgets
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

            if (budget == null)
            {
                return false;
            }

            _context.Budgets.Remove(budget);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<BudgetSummaryDto> AddExpenseToBudgetAsync(int userId, int id, AddExpenseToBudgetDto dto)
        {
            var budget = await _context.Budgets
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

            if (budget == null)
            {
                throw new ArgumentException("Budget not found");
            }

            budget.SpendAmount += dto.Amount;
            await _context.SaveChangesAsync();

            return await GetBudgetSummaryByIdAsync(userId, id);
        }

        public async Task<List<BudgetListDto>> GetOverBudgetItemsAsync(int userId, DateTime? monthYear = null)
        {
            var query = _context.Budgets
                .Include(b => b.Category)
                .Where(b => b.UserId == userId && b.SpendAmount > b.AllocatedAmount);

            if (monthYear.HasValue)
            {
                var targetDate = new DateTime(monthYear.Value.Year, monthYear.Value.Month, 1);
                query = query.Where(b => b.MonthYear.Year == targetDate.Year && 
                                        b.MonthYear.Month == targetDate.Month);
            }

            var budgets = await query
                .OrderByDescending(b => b.SpendAmount - b.AllocatedAmount)
                .ToListAsync();

            return budgets.Select(b => CreateBudgetListDto(b)).ToList();
        }

        public async Task UpdateBudgetSpendFromTransactionsAsync(int userId, int categoryId, DateTime monthYear)
        {
            var targetDate = new DateTime(monthYear.Year, monthYear.Month, 1);
            var endOfMonth = targetDate.AddMonths(1).AddDays(-1);

            // Calculate total expenses for the category in the specified month
            var totalExpenses = await _context.Transactions
                .Where(t => t.UserId == userId && 
                           t.CategoryId == categoryId && 
                           t.Type == "Expense" &&
                           t.Date >= targetDate && 
                           t.Date <= endOfMonth)
                .SumAsync(t => t.Amount);

            // Update the budget's spend amount
            var budget = await _context.Budgets
                .FirstOrDefaultAsync(b => b.UserId == userId && 
                                         b.CategoryId == categoryId && 
                                         b.MonthYear.Year == targetDate.Year && 
                                         b.MonthYear.Month == targetDate.Month);

            if (budget != null)
            {
                budget.SpendAmount = totalExpenses;
                await _context.SaveChangesAsync();
            }
        }

        private BudgetSummaryDto CreateBudgetSummaryDto(Budget budget)
        {
            var remainingAmount = budget.AllocatedAmount - budget.SpendAmount;
            var spendPercentage = budget.AllocatedAmount > 0 ? (budget.SpendAmount / budget.AllocatedAmount) * 100 : 0;
            var remainingPercentage = Math.Max(0, 100 - spendPercentage);
            var isOverBudget = budget.SpendAmount > budget.AllocatedAmount;

            string status;
            if (isOverBudget)
            {
                status = "Over Budget";
            }
            else if (spendPercentage >= 90)
            {
                status = "Near Limit";
            }
            else if (spendPercentage >= 75)
            {
                status = "On Track";
            }
            else
            {
                status = "Under Budget";
            }

            return new BudgetSummaryDto
            {
                Id = budget.Id,
                CategoryName = budget.Category?.Name ?? "Unknown",
                AllocatedAmount = budget.AllocatedAmount,
                SpendAmount = budget.SpendAmount,
                RemainingAmount = remainingAmount,
                SpendPercentage = Math.Round(spendPercentage, 2),
                RemainingPercentage = Math.Round(remainingPercentage, 2),
                Status = status,
                MonthYear = budget.MonthYear,
                IsOverBudget = isOverBudget,
                Description = budget.Description
            };
        }

        private BudgetListDto CreateBudgetListDto(Budget budget)
        {
            var spendPercentage = budget.AllocatedAmount > 0 ? (budget.SpendAmount / budget.AllocatedAmount) * 100 : 0;
            var isOverBudget = budget.SpendAmount > budget.AllocatedAmount;

            string status;
            if (isOverBudget)
            {
                status = "Over Budget";
            }
            else if (spendPercentage >= 90)
            {
                status = "Near Limit";
            }
            else if (spendPercentage >= 75)
            {
                status = "On Track";
            }
            else
            {
                status = "Under Budget";
            }

            return new BudgetListDto
            {
                Id = budget.Id,
                CategoryName = budget.Category?.Name ?? "Unknown",
                AllocatedAmount = budget.AllocatedAmount,
                SpendAmount = budget.SpendAmount,
                SpendPercentage = Math.Round(spendPercentage, 2),
                Status = status,
                MonthYear = budget.MonthYear,
                IsOverBudget = isOverBudget
            };
        }
    }
}
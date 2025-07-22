// Controllers/BudgetAnalyticsController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SpendSmart_Backend.Data;
using SpendSmart_Backend.DTOs;
using SpendSmart_Backend.DTOs.Common;
using SpendSmart_Backend.Models;

namespace SpendSmart_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BudgetAnalyticsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<BudgetAnalyticsController> _logger;

        public BudgetAnalyticsController(ApplicationDbContext context, ILogger<BudgetAnalyticsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/BudgetAnalytics/budget/{budgetId}
        [HttpGet("budget/{budgetId}")]
        public async Task<ActionResult<ApiResponseDto<BudgetAnalyticsDto>>> GetBudgetAnalytics(int budgetId)
        {
            try
            {
                var budget = await _context.Budgets
                    .Include(b => b.BudgetCategories)
                    .ThenInclude(bc => bc.Category)
                    .Include(b => b.TransactionBudgetImpacts)
                    .ThenInclude(tbi => tbi.Transaction)
                    .FirstOrDefaultAsync(b => b.BudgetId == budgetId);

                if (budget == null)
                {
                    return NotFound(ApiResponseDto<BudgetAnalyticsDto>.ErrorResponse("Budget not found"));
                }

                var currentDate = DateTime.UtcNow.Date;
                var totalDays = (budget.EndDate - budget.StartDate).Days + 1;
                var daysElapsed = Math.Max(0, (currentDate - budget.StartDate).Days + 1);
                var daysRemaining = Math.Max(0, (budget.EndDate - currentDate).Days);

                // Calculate daily rates
                var dailyBudgetRate = totalDays > 0 ? budget.TotalBudgetAmount / totalDays : 0;
                var dailySpendingRate = daysElapsed > 0 ? budget.TotalSpentAmount / daysElapsed : 0;

                // Project end amount based on current spending rate
                var projectedEndAmount = dailySpendingRate * totalDays;

                // Determine if on track (within 10% variance)
                var expectedSpentByNow = dailyBudgetRate * daysElapsed;
                var isOnTrack = Math.Abs(budget.TotalSpentAmount - expectedSpentByNow) / expectedSpentByNow <= 0.1m;

                var analytics = new BudgetAnalyticsDto
                {
                    BudgetId = budget.BudgetId,
                    BudgetName = budget.BudgetName,
                    BudgetType = budget.BudgetType,
                    StartDate = budget.StartDate,
                    EndDate = budget.EndDate,
                    TotalBudgetAmount = budget.TotalBudgetAmount,
                    TotalSpentAmount = budget.TotalSpentAmount,
                    RemainingAmount = budget.RemainingAmount,
                    ProgressPercentage = budget.ProgressPercentage,
                    DaysElapsed = daysElapsed,
                    TotalDays = totalDays,
                    DaysRemaining = daysRemaining,
                    DailyBudgetRate = dailyBudgetRate,
                    DailySpendingRate = dailySpendingRate,
                    ProjectedEndAmount = projectedEndAmount,
                    IsOnTrack = isOnTrack,
                    Status = budget.Status,
                    CategoryAnalytics = budget.BudgetCategories.Select(bc =>
                    {
                        var categoryTransactions = budget.TransactionBudgetImpacts
                            .Where(tbi => tbi.CategoryId == bc.CategoryId)
                            .ToList();

                        return new CategoryAnalyticsDto
                        {
                            CategoryId = bc.CategoryId,
                            CategoryName = bc.Category.CategoryName,
                            AllocatedAmount = bc.AllocatedAmount,
                            SpentAmount = bc.SpentAmount,
                            RemainingAmount = bc.RemainingAmount,
                            ProgressPercentage = bc.AllocatedAmount > 0 ? (bc.SpentAmount / bc.AllocatedAmount) * 100 : 0,
                            BudgetSharePercentage = budget.TotalBudgetAmount > 0 ? (bc.AllocatedAmount / budget.TotalBudgetAmount) * 100 : 0,
                            IsOverBudget = bc.SpentAmount > bc.AllocatedAmount,
                            OverBudgetAmount = Math.Max(0, bc.SpentAmount - bc.AllocatedAmount),
                            TransactionCount = categoryTransactions.Count,
                            AverageTransactionAmount = categoryTransactions.Any() ? categoryTransactions.Average(ct => ct.ImpactAmount) : 0
                        };
                    }).ToList()
                };

                return Ok(ApiResponseDto<BudgetAnalyticsDto>.SuccessResponse(analytics));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving budget analytics for budget {BudgetId}", budgetId);
                return StatusCode(500, ApiResponseDto<BudgetAnalyticsDto>.ErrorResponse("An error occurred while retrieving budget analytics"));
            }
        }

        // GET: api/BudgetAnalytics/user/{userId}/comparison
        [HttpGet("user/{userId}/comparison")]
        public async Task<ActionResult<ApiResponseDto<BudgetComparisonDto>>> GetBudgetComparison(
            int userId,
            [FromQuery] string period = "monthly") // monthly or annually
        {
            try
            {
                var currentDate = DateTime.UtcNow.Date;
                DateTime currentPeriodStart, currentPeriodEnd, previousPeriodStart, previousPeriodEnd;

                if (period.ToLower() == "monthly")
                {
                    currentPeriodStart = new DateTime(currentDate.Year, currentDate.Month, 1);
                    currentPeriodEnd = currentPeriodStart.AddMonths(1).AddDays(-1);
                    previousPeriodStart = currentPeriodStart.AddMonths(-1);
                    previousPeriodEnd = previousPeriodStart.AddMonths(1).AddDays(-1);
                }
                else // annually
                {
                    currentPeriodStart = new DateTime(currentDate.Year, 1, 1);
                    currentPeriodEnd = new DateTime(currentDate.Year, 12, 31);
                    previousPeriodStart = new DateTime(currentDate.Year - 1, 1, 1);
                    previousPeriodEnd = new DateTime(currentDate.Year - 1, 12, 31);
                }

                // Get current period data
                var currentBudgets = await GetPeriodBudgetData(userId, currentPeriodStart, currentPeriodEnd);
                var previousBudgets = await GetPeriodBudgetData(userId, previousPeriodStart, previousPeriodEnd);

                var currentTotalBudget = currentBudgets.Sum(b => b.TotalBudgetAmount);
                var currentTotalSpent = currentBudgets.Sum(b => b.TotalSpentAmount);
                var previousTotalBudget = previousBudgets.Sum(b => b.TotalBudgetAmount);
                var previousTotalSpent = previousBudgets.Sum(b => b.TotalSpentAmount);

                var spendingChangePercentage = previousTotalSpent > 0
                    ? ((currentTotalSpent - previousTotalSpent) / previousTotalSpent) * 100
                    : 0;

                var spendingTrend = spendingChangePercentage > 5 ? "Increasing"
                    : spendingChangePercentage < -5 ? "Decreasing"
                    : "Stable";

                var comparison = new BudgetComparisonDto
                {
                    Period = period.ToLower() == "monthly" ? "Current vs Previous Month" : "Current vs Previous Year",
                    CurrentPeriod = new BudgetPeriodDto
                    {
                        StartDate = currentPeriodStart,
                        EndDate = currentPeriodEnd,
                        TotalBudget = currentTotalBudget,
                        TotalSpent = currentTotalSpent,
                        UtilizationPercentage = currentTotalBudget > 0 ? (currentTotalSpent / currentTotalBudget) * 100 : 0
                    },
                    PreviousPeriod = new BudgetPeriodDto
                    {
                        StartDate = previousPeriodStart,
                        EndDate = previousPeriodEnd,
                        TotalBudget = previousTotalBudget,
                        TotalSpent = previousTotalSpent,
                        UtilizationPercentage = previousTotalBudget > 0 ? (previousTotalSpent / previousTotalBudget) * 100 : 0
                    },
                    SpendingChangePercentage = spendingChangePercentage,
                    SpendingTrend = spendingTrend,
                    CategoryComparisons = await GetCategoryComparisons(userId, currentPeriodStart, currentPeriodEnd, previousPeriodStart, previousPeriodEnd)
                };

                return Ok(ApiResponseDto<BudgetComparisonDto>.SuccessResponse(comparison));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving budget comparison for user {UserId}", userId);
                return StatusCode(500, ApiResponseDto<BudgetComparisonDto>.ErrorResponse("An error occurred while retrieving budget comparison"));
            }
        }

        // GET: api/BudgetAnalytics/user/{userId}/overview
        [HttpGet("user/{userId}/overview")]
        public async Task<ActionResult<ApiResponseDto<object>>> GetUserBudgetOverview(int userId)
        {
            try
            {
                var currentDate = DateTime.UtcNow.Date;

                // Get active budgets
                var activeBudgets = await _context.Budgets
                    .Where(b => b.UserId == userId &&
                               b.Status == "Active" &&
                               b.StartDate <= currentDate &&
                               b.EndDate >= currentDate)
                    .Include(b => b.BudgetCategories)
                    .ToListAsync();

                // Get recent transactions (last 30 days)
                var recentTransactions = await _context.Transactions
                    .Where(t => t.UserId == userId &&
                               t.TransactionDate >= currentDate.AddDays(-30))
                    .ToListAsync();

                var totalActiveBudgets = activeBudgets.Sum(b => b.TotalBudgetAmount);
                var totalActiveSpent = activeBudgets.Sum(b => b.TotalSpentAmount);
                var totalCategories = activeBudgets.SelectMany(b => b.BudgetCategories).Count();
                var overBudgetCategories = activeBudgets
                    .SelectMany(b => b.BudgetCategories)
                    .Count(bc => bc.SpentAmount > bc.AllocatedAmount);

                var overview = new
                {
                    Summary = new
                    {
                        ActiveBudgetsCount = activeBudgets.Count,
                        TotalBudgetAmount = totalActiveBudgets,
                        TotalSpentAmount = totalActiveSpent,
                        RemainingAmount = totalActiveBudgets - totalActiveSpent,
                        OverallUtilization = totalActiveBudgets > 0 ? (totalActiveSpent / totalActiveBudgets) * 100 : 0,
                        TotalCategories = totalCategories,
                        OverBudgetCategories = overBudgetCategories
                    },
                    RecentActivity = new
                    {
                        Last30DaysTransactions = recentTransactions.Count,
                        Last30DaysIncome = recentTransactions.Where(t => t.TransactionType == "Income").Sum(t => t.Amount),
                        Last30DaysExpenses = recentTransactions.Where(t => t.TransactionType == "Expense").Sum(t => t.Amount),
                        AverageTransactionAmount = recentTransactions.Any() ? recentTransactions.Average(t => t.Amount) : 0
                    },
                    Alerts = GetBudgetAlerts(activeBudgets)
                };

                return Ok(ApiResponseDto<object>.SuccessResponse(overview));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving budget overview for user {UserId}", userId);
                return StatusCode(500, ApiResponseDto<object>.ErrorResponse("An error occurred while retrieving budget overview"));
            }
        }

        private async Task<List<Budget>> GetPeriodBudgetData(int userId, DateTime startDate, DateTime endDate)
        {
            return await _context.Budgets
                .Where(b => b.UserId == userId &&
                           ((b.StartDate >= startDate && b.StartDate <= endDate) ||
                            (b.EndDate >= startDate && b.EndDate <= endDate) ||
                            (b.StartDate <= startDate && b.EndDate >= endDate)))
                .ToListAsync();
        }

        private async Task<List<CategoryComparisonDto>> GetCategoryComparisons(
            int userId,
            DateTime currentStart,
            DateTime currentEnd,
            DateTime previousStart,
            DateTime previousEnd)
        {
            var categories = await _context.Categories.ToListAsync();
            var comparisons = new List<CategoryComparisonDto>();

            foreach (var category in categories)
            {
                var currentSpending = await _context.TransactionBudgetImpacts
                    .Where(tbi => tbi.CategoryId == category.Id &&
                                 tbi.Transaction.UserId == userId &&
                                 tbi.Transaction.TransactionDate >= currentStart &&
                                 tbi.Transaction.TransactionDate <= currentEnd)
                    .SumAsync(tbi => tbi.ImpactAmount);

                var previousSpending = await _context.TransactionBudgetImpacts
                    .Where(tbi => tbi.CategoryId == category.Id &&
                                 tbi.Transaction.UserId == userId &&
                                 tbi.Transaction.TransactionDate >= previousStart &&
                                 tbi.Transaction.TransactionDate <= previousEnd)
                    .SumAsync(tbi => tbi.ImpactAmount);

                if (currentSpending > 0 || previousSpending > 0)
                {
                    var changeAmount = currentSpending - previousSpending;
                    var changePercentage = previousSpending > 0 ? (changeAmount / previousSpending) * 100 : 0;
                    var trend = changePercentage > 5 ? "Increasing"
                        : changePercentage < -5 ? "Decreasing"
                        : "Stable";

                    comparisons.Add(new CategoryComparisonDto
                    {
                        CategoryId = category.Id,
                        CategoryName = category.CategoryName,
                        CurrentSpending = currentSpending,
                        PreviousSpending = previousSpending,
                        ChangeAmount = changeAmount,
                        ChangePercentage = changePercentage,
                        Trend = trend
                    });
                }
            }

            return comparisons.OrderByDescending(c => c.CurrentSpending).ToList();
        }

        private List<object> GetBudgetAlerts(List<Budget> activeBudgets)
        {
            var alerts = new List<object>();

            foreach (var budget in activeBudgets)
            {
                // Over budget alert
                if (budget.TotalSpentAmount > budget.TotalBudgetAmount)
                {
                    alerts.Add(new
                    {
                        Type = "OverBudget",
                        BudgetId = budget.BudgetId,
                        BudgetName = budget.BudgetName,
                        Message = $"Budget '{budget.BudgetName}' is over limit by {budget.TotalSpentAmount - budget.TotalBudgetAmount:C}",
                        Severity = "High"
                    });
                }
                // Near budget limit alert (90%+)
                else if (budget.ProgressPercentage >= 90)
                {
                    alerts.Add(new
                    {
                        Type = "NearLimit",
                        BudgetId = budget.BudgetId,
                        BudgetName = budget.BudgetName,
                        Message = $"Budget '{budget.BudgetName}' is {budget.ProgressPercentage:F1}% utilized",
                        Severity = "Medium"
                    });
                }

                // Budget ending soon (within 7 days)
                var daysRemaining = (budget.EndDate - DateTime.UtcNow.Date).Days;
                if (daysRemaining <= 7 && daysRemaining > 0)
                {
                    alerts.Add(new
                    {
                        Type = "EndingSoon",
                        BudgetId = budget.BudgetId,
                        BudgetName = budget.BudgetName,
                        Message = $"Budget '{budget.BudgetName}' ends in {daysRemaining} day(s)",
                        Severity = "Low"
                    });
                }
            }

            return alerts;
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SpendSmart_Backend.Data;
using SpendSmart_Backend.DTOs;

namespace SpendSmart_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("generate")]
        public async Task<IActionResult> Generate([FromBody] ReportRequestDto request)
        {
            if (request == null)
                return BadRequest("Request body is missing.");

            if (request.StartDate == default || request.EndDate == default)
                return BadRequest("StartDate and EndDate must be valid dates.");

            if (request.StartDate > request.EndDate)
                return BadRequest("StartDate cannot be after EndDate.");

            // 🔍 Filter by UserId and Date range
            var transactions = await _context.Transactions
                .Where(t => t.UserId == request.UserId && t.Date >= request.StartDate && t.Date <= request.EndDate)
                .Include(t => t.Category)
                .ToListAsync();

            // 🧮 Aggregations and grouping remain the same...
            var totalIncome = transactions
                .Where(t => t.Type == "Income")
                .Sum(t => t.Amount);

            var totalExpenses = transactions
                .Where(t => t.Type == "Expense")
                .Sum(t => t.Amount);

            var categoryBreakdown = transactions
                .Where(t => t.Type == "Expense" && t.Category != null)
                .GroupBy(t => t.Category.Name)
                .ToDictionary(g => g.Key, g => g.Sum(t => t.Amount));

            var monthlyData = transactions
                .GroupBy(t => new { t.Date.Month, t.Type })
                .Select(g => new
                {
                    g.Key.Month,
                    g.Key.Type,
                    Total = g.Sum(t => t.Amount)
                })
                .ToList()
                .GroupBy(x => x.Month)
                .Select(g => new MonthlyIncomeExpense
                {
                    Month = new DateTime(1, g.Key, 1).ToString("MMM"),
                    Income = g.FirstOrDefault(x => x.Type == "Income")?.Total ?? 0,
                    Expenses = g.FirstOrDefault(x => x.Type == "Expense")?.Total ?? 0
                })
                .ToList();

            // 🏁 Filter user-specific goals if applicable or return all
            var goals = await _context.Goals.Where(g => g.UserId == request.UserId).ToListAsync(); // Optional: if goals are user-specific
            var goalStatuses = goals.Select(g => new GoalStatusDto
            {
                GoalName = g.Name,
                ProgressPercentage = g.TargetAmount == 0 ? 0 : (g.CurrentAmount / g.TargetAmount) * 100
            }).ToList();

            var report = new VisualReportDto
            {
                TotalIncome = totalIncome,
                TotalExpenses = totalExpenses,
                CategoryBreakdown = categoryBreakdown,
                MonthlyData = monthlyData,
                Goals = goalStatuses,
                Transactions = transactions
            };

            return Ok(report);
        }
    }
    }

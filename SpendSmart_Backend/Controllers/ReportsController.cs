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
        private readonly ILogger<ReportsController> _logger;

        public ReportsController(ApplicationDbContext context, ILogger<ReportsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost("generate")]
        public async Task<IActionResult> Generate([FromBody] ReportRequestDto request)
        {
            try
            {
                _logger.LogInformation($"Generating report for user {request.UserId} from {request.StartDate} to {request.EndDate}");

                // Basic validation
                if (request == null)
                    return BadRequest(new { message = "Request body is missing." });

                if (request.StartDate == default || request.EndDate == default)
                    return BadRequest(new { message = "StartDate and EndDate must be valid dates." });

                if (request.StartDate > request.EndDate)
                    return BadRequest(new { message = "StartDate cannot be after EndDate." });

                // For testing without auth, use default userId if not provided
                var userId = request.UserId > 0 ? request.UserId : 1;

                // 🔍 Get transactions for the date range
                var transactions = await _context.Transactions
                    .Where(t => t.UserId == userId &&
                               t.Date >= request.StartDate &&
                               t.Date <= request.EndDate)
                    .Include(t => t.Category)
                    .OrderByDescending(t => t.Date)
                    .ToListAsync();

                _logger.LogInformation($"Found {transactions.Count} transactions for the date range");

                // 🧮 Calculate totals
                var totalIncome = transactions
                    .Where(t => t.Type.ToLower() == "income")
                    .Sum(t => t.Amount);

                var totalExpenses = transactions
                    .Where(t => t.Type.ToLower() == "expense")
                    .Sum(t => t.Amount);

                var totalSavings = totalIncome - totalExpenses; // ADD THIS LINE

                // 📊 Category breakdown for expenses
                var categoryBreakdown = transactions
                    .Where(t => t.Type.ToLower() == "expense" && t.Category != null)
                    .GroupBy(t => t.Category.Name)
                    .ToDictionary(g => g.Key, g => g.Sum(t => t.Amount));

                // 📈 Monthly data aggregation
                var monthlyData = transactions
                    .GroupBy(t => new { t.Date.Year, t.Date.Month, t.Type })
                    .Select(g => new
                    {
                        g.Key.Year,
                        g.Key.Month,
                        g.Key.Type,
                        Total = g.Sum(t => t.Amount)
                    })
                    .ToList()
                    .GroupBy(x => new { x.Year, x.Month })
                    .Select(g => new MonthlyIncomeExpense
                    {
                        Month = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy"),
                        Income = g.FirstOrDefault(x => x.Type.ToLower() == "income")?.Total ?? 0,
                        Expenses = g.FirstOrDefault(x => x.Type.ToLower() == "expense")?.Total ?? 0
                    })
                    .OrderBy(x => DateTime.ParseExact(x.Month, "MMM yyyy", null))
                    .ToList();

                // 🎯 Get goals for the user
                var goals = await _context.Goals
                    .Where(g => g.UserId == userId)
                    .ToListAsync();

                var goalStatuses = goals.Select(g => new GoalStatusDto
                {
                    GoalName = g.Name,
                    CurrentAmount = g.CurrentAmount,
                    TargetAmount = g.TargetAmount,
                    ProgressPercentage = g.TargetAmount == 0 ? 0 : Math.Round((g.CurrentAmount / g.TargetAmount) * 100, 2),
                    EndDate = g.EndDate
                }).ToList();

                // 📋 Format transactions for frontend
                var formattedTransactions = transactions.Select(t => new
                {
                    id = t.Id,
                    date = t.Date.ToString("yyyy-MM-dd"),
                    category = t.Category?.Name ?? "Uncategorized",
                    amount = t.Amount,
                    type = t.Type,
                    description = t.Description ?? ""
                }).ToList();

                // 📊 Simple budget utilization calculation
                var budgetUtilization = totalExpenses > 0 ? Math.Round((totalExpenses / Math.Max(totalIncome, 1)) * 100, 2) : 0;

                // 💰 ADD SAVINGS GROWTH CALCULATION
                var savingsGrowthOverTime = await CalculateSavingsGrowthOverTime(
                    userId, request.StartDate, request.EndDate);

                var report = new VisualReportDto
                {
                    TotalIncome = totalIncome,
                    TotalExpenses = totalExpenses,
                    TotalSavings = totalSavings, // ADD THIS LINE
                    BudgetUtilization = budgetUtilization,
                    CategoryBreakdown = categoryBreakdown,
                    MonthlyData = monthlyData,
                    Goals = goalStatuses,
                    Transactions = formattedTransactions.Cast<object>().ToList(),
                    SavingsGrowthOverTime = savingsGrowthOverTime,// ADD THIS LINE
                };

                _logger.LogInformation($"Report generated successfully with {formattedTransactions.Count} transactions and {savingsGrowthOverTime.Count} savings entries");
                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating report");
                return StatusCode(500, new { message = "An error occurred while generating the report", error = ex.Message });
            }
        }

        // ADD THIS NEW METHOD FOR SAVINGS GROWTH CALCULATION
        private async Task<List<SavingsGrowthDataDto>> CalculateSavingsGrowthOverTime(
            int userId, DateTime startDate, DateTime endDate)
        {
            var result = new List<SavingsGrowthDataDto>();

            try
            {
                var months = GetMonthsBetweenDates(startDate, endDate);
                decimal cumulativeSavings = 0;

                foreach (var (year, month) in months)
                {
                    var monthStart = new DateTime(year, month, 1);
                    var monthEnd = monthStart.AddMonths(1).AddDays(-1);

                    // Get transactions for this month
                    var monthlyTransactions = await _context.Transactions
                        .Where(t => t.UserId == userId &&
                                   t.Date >= monthStart &&
                                   t.Date <= monthEnd)
                        .ToListAsync();

                    // Calculate monthly income and expenses
                    var monthlyIncome = monthlyTransactions
                        .Where(t => t.Type.ToLower() == "income")
                        .Sum(t => t.Amount);

                    var monthlyExpenses = monthlyTransactions
                        .Where(t => t.Type.ToLower() == "expense")
                        .Sum(t => t.Amount);

                    // Calculate monthly savings
                    var monthlySavings = monthlyIncome - monthlyExpenses;

                    // Update cumulative savings
                    cumulativeSavings += monthlySavings;

                    // Calculate savings rate
                    var savingsRate = monthlyIncome > 0 ?
                        (monthlySavings / monthlyIncome) * 100 : 0;

                    result.Add(new SavingsGrowthDataDto
                    {
                        Month = $"{GetMonthName(month)} {year}",
                        MonthlySavings = monthlySavings,
                        CumulativeSavings = cumulativeSavings,
                        Income = monthlyIncome,
                        Expenses = monthlyExpenses,
                        SavingsRate = Math.Round(savingsRate, 1),
                        MonthDate = new DateTime(year, month, 1).ToString("yyyy-MM-dd")
                    });
                }

                return result.OrderBy(r => DateTime.Parse(r.MonthDate)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error calculating savings growth for user {userId}");
                return new List<SavingsGrowthDataDto>();
            }
        }

        // ADD THESE HELPER METHODS
        private List<(int year, int month)> GetMonthsBetweenDates(DateTime startDate, DateTime endDate)
        {
            var months = new List<(int year, int month)>();
            var current = new DateTime(startDate.Year, startDate.Month, 1);
            var end = new DateTime(endDate.Year, endDate.Month, 1);

            while (current <= end)
            {
                months.Add((current.Year, current.Month));
                current = current.AddMonths(1);
            }

            return months;
        }

        private string GetMonthName(int month)
        {
            return new DateTime(2000, month, 1).ToString("MMM");
        }

        // Test endpoint to verify API is working
        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok(new
            {
                message = "Reports API is working!",
                timestamp = DateTime.Now,
                environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown"
            });
        }

        // GET: api/Reports/stored/{userId}
        [HttpGet("stored/{userId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetStoredReports(int userId)
        {
            try
            {
                _logger.LogInformation($"Retrieving stored reports for user {userId}");

                var reports = await _context.Reports
                    .Where(r => r.UserId == userId && r.Status == "Active")
                    .OrderByDescending(r => r.DateGenerated)
                    .Select(r => new
                    {
                        r.Id,
                        r.ReportName,
                        r.Format,
                        r.DateGenerated,
                        r.StartDate,
                        r.EndDate,
                        r.FirebaseUrl,
                        r.Description,
                        r.FileSizeBytes,
                        r.FileName,
                        r.AccessCount,
                        r.LastAccessed,
                        r.Status,
                        DateRange = $"{r.StartDate:yyyy-MM-dd} to {r.EndDate:yyyy-MM-dd}",
                        FileSizeMB = r.FileSizeBytes.HasValue ? Math.Round((double)r.FileSizeBytes.Value / 1024 / 1024, 2) : (double?)null
                    })
                    .ToListAsync();

                _logger.LogInformation($"Found {reports.Count} stored reports for user {userId}");
                return Ok(reports);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving stored reports for user {userId}");
                return StatusCode(500, new { message = "Error retrieving stored reports", error = ex.Message });
            }
        }

        // POST: api/Reports/store
        [HttpPost("store")]
        public async Task<ActionResult<object>> StoreReport([FromBody] StoreReportDto reportDto)
        {
            try
            {
                _logger.LogInformation($"Storing report for user {reportDto.UserId}: {reportDto.ReportName}");

                var report = new Models.Report
                {
                    ReportName = reportDto.ReportName,
                    Format = reportDto.Format,
                    DateGenerated = DateTime.UtcNow,
                    StartDate = reportDto.StartDate,
                    EndDate = reportDto.EndDate,
                    FirebaseUrl = reportDto.FirebaseUrl,
                    Description = reportDto.Description,
                    UserId = reportDto.UserId,
                    // Enhanced metadata
                    FileSizeBytes = reportDto.FileSizeBytes,
                    FileName = reportDto.FileName,
                    AccessCount = 0,
                    Status = "Active"
                };

                _context.Reports.Add(report);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Report stored successfully with ID {report.Id}");

                return Ok(new
                {
                    id = report.Id,
                    message = "Report stored successfully",
                    reportName = report.ReportName,
                    dateGenerated = report.DateGenerated,
                    fileSizeBytes = report.FileSizeBytes,
                    fileName = report.FileName
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error storing report for user {reportDto.UserId}");
                return StatusCode(500, new { message = "Error storing report", error = ex.Message });
            }
        }

        // DELETE: api/Reports/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStoredReport(int id)
        {
            try
            {
                _logger.LogInformation($"Deleting report with ID {id}");

                var report = await _context.Reports.FindAsync(id);
                if (report == null)
                {
                    return NotFound(new { message = "Report not found" });
                }

                _context.Reports.Remove(report);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Report with ID {id} deleted successfully");
                return Ok(new { message = "Report deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting report with ID {id}");
                return StatusCode(500, new { message = "Error deleting report", error = ex.Message });
            }
        }

        // Add sample data endpoint for testing
        [HttpPost("sample-data")]
        public async Task<IActionResult> CreateSampleData()
        {
            try
            {
                // Create sample user if doesn't exist
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == 1);
                if (user == null)
                {
                    // This assumes you have a User entity - adjust as needed
                    return BadRequest(new { message = "Please create a user with ID 1 first for testing" });
                }

                // Check if sample data already exists
                var existingTransactions = await _context.Transactions.Where(t => t.UserId == 1).CountAsync();
                if (existingTransactions > 0)
                {
                    return Ok(new { message = $"Sample data already exists ({existingTransactions} transactions)" });
                }

                // Create sample categories
                var categories = new[]
                {
                    new { Name = "Foods", Type = "Expense" },
                    new { Name = "Transport", Type = "Expense" },
                    new { Name = "Groceries", Type = "Expense" },
                    new { Name = "Entertainment", Type = "Expense" },
                    new { Name = "Salary", Type = "Income" },
                    new { Name = "Freelance", Type = "Income" }
                };

                // Add sample transactions, goals, etc.
                // Note: Adjust this based on your actual entity models

                return Ok(new { message = "Sample data creation endpoint - implement based on your models" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating sample data");
                return StatusCode(500, new { message = "Error creating sample data", error = ex.Message });
            }
        }
    }
}
using Microsoft.EntityFrameworkCore;
using SpendSmart_Backend.Data;
using SpendSmart_Backend.DTOs;

namespace SpendSmart_Backend.Services
{

    public class DashboardService: IDashboardService

    {
        private readonly ApplicationDbContext _context;

        public DashboardService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardSummaryDto> GetDashboardSummary(int userId)
        {
            var transactions = await _context.Transactions
                .Where(t => t.UserId == userId)
                .Include(t => t.Category)
                .ToListAsync();

            var income = transactions.Where(t => t.Type == "Income").Sum(t => t.Amount);
            var expense = transactions.Where(t => t.Type == "Expense").Sum(t => t.Amount);
            var balance = income - expense;

            var recentTransactions = transactions
                .OrderByDescending(t => t.Date)
                .Take(5)
                .Select(t => new TransactionViewDto
                {
                    Id = t.Id,
                    Type = t.Type,
                    Category = t.Category.Name,
                    Amount = t.Amount,
                    Date = t.Date.ToString("yyyy-MM-dd"),
                    Description = t.Description,
                })
                .ToList();

            return new DashboardSummaryDto
            {
                Income = income,
                Expense = expense,
                Balance = balance,
                RecentTransactions = recentTransactions,
            };
        }
        public async Task<List<DashboardIncomeExpenseDto>> GetIncomeVsExpenseSummary(int userId, string Period)
        {
            var transactions = _context.Transactions
                .Where(t => t.UserId == userId)
                .Include(t => t.Category)
                .AsQueryable();


            if(Period == "Monthly")

            {
                return await transactions
                    .GroupBy(t => new { t.Date.Year, t.Date.Month })
                    .Select(g => new DashboardIncomeExpenseDto
                    {
                        Period = $"{g.Key.Year}-{g.Key.Month}",
                        Income = g.Where(t => t.Type == "Income").Sum(t => t.Amount),
                        Expense = g.Where(t => t.Type == "Expense").Sum(t => t.Amount)
                    }).ToListAsync();
            }
            else if (Period == "Yearly")
            {
                return await transactions
                    .GroupBy(t => t.Date.Year)
                    .Select(g => new DashboardIncomeExpenseDto
                    {
                        Period = g.Key.ToString(),
                        Income = g.Where(t => t.Type == "Income").Sum(t => t.Amount),
                        Expense = g.Where(t => t.Type == "Expense").Sum(t => t.Amount)
                    }).ToListAsync();
            }
            return new List<DashboardIncomeExpenseDto>();
        }

        public async Task<List<DashboardPiechartDto>> GetPiechartData(int userId)
        {
            var result = await _context.Transactions
                .Where(t => t.UserId == userId && t.Type == "Expense")
                .Include(t => t.Category)

                .GroupBy(t => new {t.CategoryId, t.Category.Name})

                .Select(g => new DashboardPiechartDto
                {
                    Label = g.Key.Name,
                    Value = g.Sum(t => t.Amount)
                })
                .ToListAsync();

            return result;
        }
    }

}


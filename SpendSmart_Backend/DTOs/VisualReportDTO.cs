using SpendSmart_Backend.Models;

namespace SpendSmart_Backend.DTOs
{
    public class VisualReportDto
    {
        public decimal TotalIncome { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal TotalSavings => TotalIncome - TotalExpenses;

        public Dictionary<string, decimal> CategoryBreakdown { get; set; }
        public List<MonthlyIncomeExpense> MonthlyData { get; set; }
        public List<GoalStatusDto> Goals { get; set; }
        public List<Transaction> Transactions { get; set; }
    }
}

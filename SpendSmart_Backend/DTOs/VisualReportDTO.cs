namespace SpendSmart_Backend.DTOs
{
    public class VisualReportDto
    {
        public decimal TotalIncome { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal TotalSavings { get; set; }
        public decimal BudgetUtilization { get; set; }
        public Dictionary<string, decimal> CategoryBreakdown { get; set; } = new();
        public List<MonthlyIncomeExpense> MonthlyData { get; set; } = new();
        public List<GoalStatusDto> Goals { get; set; } = new();
        public List<object> Transactions { get; set; } = new();

        public List<SavingsGrowthDataDto> SavingsGrowthOverTime { get; set; }


    }
}
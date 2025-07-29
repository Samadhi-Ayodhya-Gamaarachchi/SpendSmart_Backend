namespace SpendSmart_Backend.DTOs
{
    public class DashboardIncomeExpenseDto
    {
        public string Period { get; set; } = string.Empty;
        public decimal Income { get; set; }
        public decimal Expense { get; set; }
    }
}
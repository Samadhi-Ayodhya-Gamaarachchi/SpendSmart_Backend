namespace SpendSmart_Backend.DTOs
{
    public class MonthlyIncomeExpense
    {
        public string Month { get; set; } = string.Empty;
        public decimal Income { get; set; }
        public decimal Expenses { get; set; }
    }
}
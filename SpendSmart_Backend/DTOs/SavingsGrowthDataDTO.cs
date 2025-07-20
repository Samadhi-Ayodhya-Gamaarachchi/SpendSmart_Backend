namespace SpendSmart_Backend.DTOs
{
    public class SavingsGrowthDataDto
    {
        public string Month { get; set; }
        public decimal MonthlySavings { get; set; }
        public decimal CumulativeSavings { get; set; }
        public decimal Income { get; set; }
        public decimal Expenses { get; set; }
        public decimal SavingsRate { get; set; } // percentage
        public string MonthDate { get; set; }
    }
}
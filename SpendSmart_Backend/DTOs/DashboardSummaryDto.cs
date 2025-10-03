namespace SpendSmart_Backend.DTOs
{
    public class DashboardSummaryDto
    {
        public decimal Income { get; set; }
        public decimal Expense { get; set; }
        public decimal Balance { get; set; }
        public List<TransactionViewDto> RecentTransactions { get; set; } = new List<TransactionViewDto>();
    }
}

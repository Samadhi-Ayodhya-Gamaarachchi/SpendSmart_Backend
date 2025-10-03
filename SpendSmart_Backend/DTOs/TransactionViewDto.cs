namespace SpendSmart_Backend.DTOs
{
    public class TransactionViewDto
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public string Category { get; set; }
        public decimal Amount { get; set; }
        public string Date { get; set; }
        public string? Description { get; set; }


    }
}

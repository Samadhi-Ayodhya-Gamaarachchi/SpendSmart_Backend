
﻿namespace SpendSmart_Backend.DTOs
{
    public class TransactionDto
    {
        public string Type { get; set; }
        public int CategoryId { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string? Description { get; set; }
    }
}


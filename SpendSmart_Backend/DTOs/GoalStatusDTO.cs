namespace SpendSmart_Backend.DTOs
{
    public class GoalStatusDto
    {
        public string GoalName { get; set; } = string.Empty;
        public decimal ProgressPercentage { get; set; }
        public decimal CurrentAmount { get; set; }
        public decimal TargetAmount { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
namespace SpendSmart_Backend.Models.DTOs
{
    public class EmailVerificationResponseDto
    {
        public bool EmailVerificationRequired { get; set; }
        public string? PendingEmail { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool Success { get; set; }
    }
}

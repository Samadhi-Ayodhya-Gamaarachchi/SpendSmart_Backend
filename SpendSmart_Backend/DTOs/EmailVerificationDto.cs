namespace SpendSmart_Backend.DTOs
{
    public class EmailVerificationDto
    {
        public int UserId { get; set; }
        public string Token { get; set; } = string.Empty;
    }
}

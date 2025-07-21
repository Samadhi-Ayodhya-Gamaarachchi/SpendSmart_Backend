namespace SpendSmart_Backend.Services
{
    public interface IEmailService
    {
        Task<bool> SendEmailVerificationAsync(string toEmail, string verificationToken, string userName);
        Task<bool> SendEmailChangeVerificationAsync(string newEmail, string verificationToken, string userName);
    }
}

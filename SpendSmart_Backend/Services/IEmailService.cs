using SpendSmart_Backend.Models;

namespace SpendSmart_Backend.Services
{
    public interface IEmailService
    {
        Task SendVerificationEmailAsync(EmailVerification verification);
        Task SendEmailChangeNotificationAsync(string oldEmail, string newEmail, string adminName);
    }
}

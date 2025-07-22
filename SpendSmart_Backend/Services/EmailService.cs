using System.Net;
using System.Net.Mail;
using SpendSmart_Backend.Models;

namespace SpendSmart_Backend.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;
        private readonly INotificationService _notificationService;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger, INotificationService notificationService)
        {
            _configuration = configuration;
            _logger = logger;
            _notificationService = notificationService;
        }

        public async Task SendVerificationEmailAsync(EmailVerification verification)
        {
            try
            {
                var smtpSettings = _configuration.GetSection("EmailSettings");
                var smtpClient = new SmtpClient(smtpSettings["SmtpServer"])
                {
                    Port = int.Parse(smtpSettings["SmtpPort"] ?? "587"),
                    Credentials = new NetworkCredential(
                        smtpSettings["Username"], 
                        smtpSettings["Password"]
                    ),
                    EnableSsl = bool.Parse(smtpSettings["EnableSsl"] ?? "true")
                };

                var verificationUrl = $"{_configuration["AppSettings:FrontendUrl"]}/verify-email/{verification.VerificationToken}";
                
                var mailMessage = new MailMessage
                {
                    From = new MailAddress(smtpSettings["FromEmail"] ?? "noreply@spendsmart.com", "SpendSmart Admin"),
                    Subject = "SpendSmart - Verify Your New Email Address",
                    Body = CreateVerificationEmailBody(verification.NewEmail, verificationUrl),
                    IsBodyHtml = true
                };

                mailMessage.To.Add(verification.NewEmail);

                await smtpClient.SendMailAsync(mailMessage);
                _logger.LogInformation($"Verification email sent successfully to {verification.NewEmail}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send verification email to {verification.NewEmail}");
                
                // 🔔 CREATE NOTIFICATION FOR EMAIL SERVICE FAILURE
                await _notificationService.CreateEmailServiceFailureNotificationAsync(
                    verification.NewEmail, 
                    ex.Message
                );
                
                throw new InvalidOperationException("Failed to send verification email", ex);
            }
        }

        public async Task SendEmailChangeNotificationAsync(string oldEmail, string newEmail, string adminName)
        {
            try
            {
                var smtpSettings = _configuration.GetSection("EmailSettings");
                var smtpClient = new SmtpClient(smtpSettings["SmtpServer"])
                {
                    Port = int.Parse(smtpSettings["SmtpPort"] ?? "587"),
                    Credentials = new NetworkCredential(
                        smtpSettings["Username"], 
                        smtpSettings["Password"]
                    ),
                    EnableSsl = bool.Parse(smtpSettings["EnableSsl"] ?? "true")
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(smtpSettings["FromEmail"] ?? "noreply@spendsmart.com", "SpendSmart Admin"),
                    Subject = "SpendSmart - Email Address Successfully Changed",
                    Body = CreateEmailChangeNotificationBody(oldEmail, newEmail, adminName),
                    IsBodyHtml = true
                };

                mailMessage.To.Add(oldEmail);

                await smtpClient.SendMailAsync(mailMessage);
                _logger.LogInformation($"Email change notification sent to {oldEmail}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send email change notification to {oldEmail}");
                
                // 🔔 CREATE NOTIFICATION FOR EMAIL SERVICE FAILURE
                await _notificationService.CreateEmailServiceFailureNotificationAsync(
                    oldEmail, 
                    $"Email change notification failed: {ex.Message}"
                );
                
                // Don't throw here - this is just a notification
            }
        }

        private string CreateVerificationEmailBody(string newEmail, string verificationUrl)
        {
            return $@"
                <html>
                <body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <div style='background-color: #f8f9fa; padding: 20px; border-radius: 10px;'>
                        <h2 style='color: #2c5aa0; text-align: center;'>SpendSmart Admin Panel</h2>
                        <h3 style='color: #333;'>Email Verification Required</h3>
                        
                        <p>Hello,</p>
                        
                        <p>You have requested to change your email address to: <strong>{newEmail}</strong></p>
                        
                        <p>To complete this change, please click the verification link below:</p>
                        
                        <div style='text-align: center; margin: 30px 0;'>
                            <a href='{verificationUrl}' 
                               style='background-color: #2c5aa0; color: white; padding: 12px 30px; 
                                      text-decoration: none; border-radius: 5px; display: inline-block;'>
                                Verify Email Address
                            </a>
                        </div>
                        
                        <p><strong>Important:</strong></p>
                        <ul>
                            <li>This link will expire in 24 hours</li>
                            <li>If you didn't request this change, please ignore this email</li>
                            <li>Your current email will remain active until verification is complete</li>
                        </ul>
                        
                        <hr style='margin: 30px 0; border: 1px solid #ddd;'>
                        
                        <p style='font-size: 12px; color: #666; text-align: center;'>
                            This is an automated message from SpendSmart Admin Panel. Please do not reply to this email.
                        </p>
                    </div>
                </body>
                </html>";
        }

        private string CreateEmailChangeNotificationBody(string oldEmail, string newEmail, string adminName)
        {
            return $@"
                <html>
                <body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <div style='background-color: #f8f9fa; padding: 20px; border-radius: 10px;'>
                        <h2 style='color: #2c5aa0; text-align: center;'>SpendSmart Admin Panel</h2>
                        <h3 style='color: #28a745;'>Email Address Successfully Changed</h3>
                        
                        <p>Hello {adminName},</p>
                        
                        <p>This is to confirm that your email address has been successfully changed:</p>
                        
                        <div style='background-color: #e9ecef; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                            <p><strong>Previous Email:</strong> {oldEmail}</p>
                            <p><strong>New Email:</strong> {newEmail}</p>
                            <p><strong>Changed At:</strong> {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC</p>
                        </div>
                        
                        <p>If you did not authorize this change, please contact your system administrator immediately.</p>
                        
                        <hr style='margin: 30px 0; border: 1px solid #ddd;'>
                        
                        <p style='font-size: 12px; color: #666; text-align: center;'>
                            This is an automated message from SpendSmart Admin Panel. Please do not reply to this email.
                        </p>
                    </div>
                </body>
                </html>";
        }
    }
}

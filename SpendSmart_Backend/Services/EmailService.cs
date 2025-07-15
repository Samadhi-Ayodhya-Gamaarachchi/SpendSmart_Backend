using Microsoft.Extensions.Configuration;
using SpendSmart_Backend.Models;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace SpendSmart_Backend.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var smtpClient = new SmtpClient(_config["EmailSettings:SmtpServer"])
            {
                Port = int.Parse(_config["EmailSettings:Port"]),
                Credentials = new NetworkCredential(
                    _config["EmailSettings:SenderEmail"],
                    _config["EmailSettings:Password"]
                ),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_config["EmailSettings:SenderEmail"], _config["EmailSettings:SenderName"]),
                Subject = subject,
                Body = body,
                IsBodyHtml = true,
            };

            mailMessage.To.Add(toEmail);
            try
            {
                await smtpClient.SendMailAsync(mailMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Email sending failed: " + ex.Message);
                throw; // rethrow or handle appropriately
            }

        }

        public async Task SendVerificationEmailAsync(string toEmail, string token, string userName)
        {
            string verificationLink = $"http://localhost:5173/verify-email?email={WebUtility.UrlEncode(toEmail)}&token={token}";


            string subject = "Verify your email address";
            string body = $@"
                    <p>Hello {userName},</p>
                    <p>Thanks for registering at SpendSmart. Please verify your email address by clicking the link below:</p>
                    <p><a href='{verificationLink}'>Verify Email</a></p>
                    <p>If you didn’t register, please ignore this message.</p>";

            await SendEmailAsync(toEmail, subject, body);
        }
    }
}

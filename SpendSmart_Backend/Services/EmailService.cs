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

            string subject = "🎉 Welcome to SpendSmart! Verify your email";
            string body = $@"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Welcome to SpendSmart</title>
    <style>
        body {{
            margin: 0;
            padding: 0;
            font-family: 'Arial', 'Helvetica', sans-serif;
            background-color: #f5f5f5;
            line-height: 1.6;
        }}
        .container {{
            max-width: 600px;
            margin: 0 auto;
            background-color: #ffffff;
            box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
            border-radius: 12px;
            overflow: hidden;
        }}
        .header {{
            background: linear-gradient(135deg, #4CAF50 0%, #45a049 100%);
            padding: 30px 20px;
            text-align: center;
            color: white;
        }}
        .logo {{
            font-size: 32px;
            font-weight: bold;
            margin-bottom: 10px;
            display: flex;
            align-items: center;
            justify-content: center;
            gap: 10px;
        }}
        .logo-icon {{
            font-size: 40px;
        }}
        .tagline {{
            font-size: 16px;
            opacity: 0.9;
            margin: 0;
        }}
        .content {{
            padding: 40px 30px;
            color: #333333;
        }}
        .greeting {{
            font-size: 20px;
            color: #2e7d32;
            margin-bottom: 20px;
            font-weight: 600;
        }}
        .welcome-message {{
            background: linear-gradient(135deg, #e8f5e8 0%, #f1f8e9 100%);
            padding: 20px;
            border-radius: 8px;
            margin: 20px 0;
            border-left: 4px solid #4CAF50;
        }}
        .message {{
            font-size: 16px;
            margin-bottom: 20px;
            color: #555555;
        }}
        .cta-button {{
            display: inline-block;
            padding: 16px 32px;
            background: linear-gradient(135deg, #4CAF50 0%, #45a049 100%);
            color: #ffffff !important;
            text-decoration: none;
            border-radius: 8px;
            font-weight: 600;
            font-size: 16px;
            box-shadow: 0 4px 12px rgba(76, 175, 80, 0.3);
            transition: all 0.3s ease;
            margin: 20px 0;
        }}
        .features {{
            display: flex;
            justify-content: space-around;
            margin: 30px 0;
            flex-wrap: wrap;
        }}
        .feature {{
            text-align: center;
            margin: 10px;
            flex: 1;
            min-width: 150px;
        }}
        .feature-icon {{
            font-size: 32px;
            margin-bottom: 10px;
        }}
        .footer {{
            background-color: #f8f9fa;
            padding: 30px 20px;
            text-align: center;
            color: #666666;
            font-size: 14px;
            border-top: 1px solid #e9ecef;
        }}
        .social-icons {{
            font-size: 20px;
            margin: 15px 0;
        }}
        .social-icons span {{
            margin: 0 8px;
        }}
        @media (max-width: 600px) {{
            .container {{
                margin: 10px;
                border-radius: 8px;
            }}
            .content {{
                padding: 25px 20px;
            }}
            .features {{
                flex-direction: column;
            }}
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <div class='logo'>
                <span class='logo-icon'>💰</span>
                <span>SpendSmart</span>
            </div>
            <p class='tagline'>Smart Financial Management</p>
        </div>
        
        <div class='content'>
            <div class='greeting'>
                🎉 Welcome to SpendSmart, {userName}!
            </div>
            
            <div class='welcome-message'>
                <strong>Thank you for joining SpendSmart!</strong> You're about to take control of your financial future with our smart expense tracking and budget management tools.
            </div>
            
            <div class='message'>
                To get started and secure your account, please verify your email address by clicking the button below:
            </div>
            
            <div style='text-align: center;'>
                <a href='{verificationLink}' class='cta-button'>
                    ✅ Verify Email Address
                </a>
            </div>
            
            <div class='features'>
                <div class='feature'>
                    <div class='feature-icon'>📊</div>
                    <strong>Track Expenses</strong><br>
                    <small>Monitor your spending in real-time</small>
                </div>
                <div class='feature'>
                    <div class='feature-icon'>🎯</div>
                    <strong>Set Goals</strong><br>
                    <small>Achieve your financial targets</small>
                </div>
                <div class='feature'>
                    <div class='feature-icon'>📈</div>
                    <strong>View Reports</strong><br>
                    <small>Insights into your finances</small>
                </div>
            </div>
            
            <div class='message' style='margin-top: 30px; font-size: 14px; color: #888888;'>
                Having trouble with the button? Copy and paste this link into your browser:<br>
                <span style='word-break: break-all; color: #4CAF50;'>{verificationLink}</span>
            </div>
        </div>
        
        <div class='footer'>
            <div class='social-icons'>
                <span>💼</span>
                <span>📊</span>
                <span>💳</span>
                <span>📈</span>
            </div>
            
            <p><strong>SpendSmart Team</strong></p>
            <p>Making financial management simple and smart</p>
            
            <p style='margin-top: 20px; font-size: 12px; color: #999999;'>
                © 2025 SpendSmart. All rights reserved.<br>
                This is an automated message, please do not reply to this email.
            </p>
        </div>
    </div>
</body>
</html>";

            await SendEmailAsync(toEmail, subject, body);
        }

        public async Task SendAdminVerificationEmailAsync(string toEmail, string token, string userName)
        {
            string verificationLink = $"http://localhost:5173/admin/verification?email={WebUtility.UrlEncode(toEmail)}&token={token}";

            string subject = "👨‍💼 SpendSmart Admin Access - Verify your email";
            string body = $@"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Admin Verification - SpendSmart</title>
    <style>
        body {{
            margin: 0;
            padding: 0;
            font-family: 'Arial', 'Helvetica', sans-serif;
            background-color: #f5f5f5;
            line-height: 1.6;
        }}
        .container {{
            max-width: 600px;
            margin: 0 auto;
            background-color: #ffffff;
            box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
            border-radius: 12px;
            overflow: hidden;
        }}
        .header {{
            background: linear-gradient(135deg, #1976d2 0%, #1565c0 100%);
            padding: 30px 20px;
            text-align: center;
            color: white;
        }}
        .logo {{
            font-size: 32px;
            font-weight: bold;
            margin-bottom: 10px;
            display: flex;
            align-items: center;
            justify-content: center;
            gap: 10px;
        }}
        .logo-icon {{
            font-size: 40px;
        }}
        .tagline {{
            font-size: 16px;
            opacity: 0.9;
            margin: 0;
        }}
        .admin-badge {{
            background-color: rgba(255, 255, 255, 0.2);
            padding: 8px 16px;
            border-radius: 20px;
            font-size: 14px;
            font-weight: 600;
            margin-top: 10px;
            display: inline-block;
        }}
        .content {{
            padding: 40px 30px;
            color: #333333;
        }}
        .greeting {{
            font-size: 20px;
            color: #1565c0;
            margin-bottom: 20px;
            font-weight: 600;
        }}
        .admin-welcome {{
            background: linear-gradient(135deg, #e3f2fd 0%, #f1f8ff 100%);
            padding: 20px;
            border-radius: 8px;
            margin: 20px 0;
            border-left: 4px solid #1976d2;
        }}
        .message {{
            font-size: 16px;
            margin-bottom: 20px;
            color: #555555;
        }}
        .cta-button {{
            display: inline-block;
            padding: 16px 32px;
            background: linear-gradient(135deg, #1976d2 0%, #1565c0 100%);
            color: #ffffff !important;
            text-decoration: none;
            border-radius: 8px;
            font-weight: 600;
            font-size: 16px;
            box-shadow: 0 4px 12px rgba(25, 118, 210, 0.3);
            transition: all 0.3s ease;
            margin: 20px 0;
        }}
        .admin-features {{
            display: flex;
            justify-content: space-around;
            margin: 30px 0;
            flex-wrap: wrap;
        }}
        .feature {{
            text-align: center;
            margin: 10px;
            flex: 1;
            min-width: 150px;
        }}
        .feature-icon {{
            font-size: 32px;
            margin-bottom: 10px;
        }}
        .security-notice {{
            background-color: #fff3e0;
            border-left: 4px solid #ff9800;
            padding: 16px;
            margin: 25px 0;
            border-radius: 4px;
        }}
        .footer {{
            background-color: #f8f9fa;
            padding: 30px 20px;
            text-align: center;
            color: #666666;
            font-size: 14px;
            border-top: 1px solid #e9ecef;
        }}
        @media (max-width: 600px) {{
            .container {{
                margin: 10px;
                border-radius: 8px;
            }}
            .content {{
                padding: 25px 20px;
            }}
            .admin-features {{
                flex-direction: column;
            }}
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <div class='logo'>
                <span class='logo-icon'>💰</span>
                <span>SpendSmart</span>
            </div>
            <div class='admin-badge'>🔐 ADMIN ACCESS</div>
            <p class='tagline'>Administrative Panel</p>
        </div>
        
        <div class='content'>
            <div class='greeting'>
                👨‍💼 Welcome Admin {userName}!
            </div>
            
            <div class='admin-welcome'>
                <strong>Administrative Access Granted</strong><br>
                You have been granted administrative privileges for the SpendSmart platform. Please verify your email to activate your admin account.
            </div>
            
            <div class='message'>
                To activate your administrative access and secure your account, please verify your email address:
            </div>
            
            <div style='text-align: center;'>
                <a href='{verificationLink}' class='cta-button'>
                    🔓 Verify Admin Email
                </a>
            </div>
            
            <div class='admin-features'>
                <div class='feature'>
                    <div class='feature-icon'>👥</div>
                    <strong>User Management</strong><br>
                    <small>Manage user accounts</small>
                </div>
                <div class='feature'>
                    <div class='feature-icon'>📊</div>
                    <strong>Analytics</strong><br>
                    <small>Platform insights</small>
                </div>
                <div class='feature'>
                    <div class='feature-icon'>⚙️</div>
                    <strong>Settings</strong><br>
                    <small>System configuration</small>
                </div>
            </div>
            
            <div class='security-notice'>
                <strong>🔒 Security Notice:</strong> Admin accounts have elevated privileges. Please keep your credentials secure and report any suspicious activity immediately.
            </div>
            
            <div class='message' style='margin-top: 30px; font-size: 14px; color: #888888;'>
                Having trouble with the button? Copy and paste this link into your browser:<br>
                <span style='word-break: break-all; color: #1976d2;'>{verificationLink}</span>
            </div>
        </div>
        
        <div class='footer'>
            <p><strong>SpendSmart Administrative Team</strong></p>
            <p>Secure platform management</p>
            
            <p style='margin-top: 20px; font-size: 12px; color: #999999;'>
                © 2025 SpendSmart. All rights reserved.<br>
                This is an automated message, please do not reply to this email.
            </p>
        </div>
    </div>
</body>
</html>";

            await SendEmailAsync(toEmail, subject, body);
        }

        public async Task SendEmailChangeVerificationAsync(string newEmail, string token, string userName, int userId)
        {
            // Use the backend API endpoint for email change verification
            string verificationLink = $"http://localhost:5110/api/EmailVerification/verify-change?userId={userId}&token={token}";

            string subject = "🔐 Verify your new email address - SpendSmart";
            string body = $@"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Verify Your Email - SpendSmart</title>
    <style>
        body {{
            margin: 0;
            padding: 0;
            font-family: 'Arial', 'Helvetica', sans-serif;
            background-color: #f5f5f5;
            line-height: 1.6;
        }}
        .container {{
            max-width: 600px;
            margin: 0 auto;
            background-color: #ffffff;
            box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
            border-radius: 12px;
            overflow: hidden;
        }}
        .header {{
            background: linear-gradient(135deg, #4CAF50 0%, #45a049 100%);
            padding: 30px 20px;
            text-align: center;
            color: white;
        }}
        .logo {{
            font-size: 32px;
            font-weight: bold;
            margin-bottom: 10px;
            display: flex;
            align-items: center;
            justify-content: center;
            gap: 10px;
        }}
        .logo-icon {{
            font-size: 40px;
        }}
        .tagline {{
            font-size: 16px;
            opacity: 0.9;
            margin: 0;
        }}
        .content {{
            padding: 40px 30px;
            color: #333333;
        }}
        .greeting {{
            font-size: 20px;
            color: #2e7d32;
            margin-bottom: 20px;
            font-weight: 600;
        }}
        .message {{
            font-size: 16px;
            margin-bottom: 30px;
            color: #555555;
        }}
        .cta-button {{
            display: inline-block;
            padding: 16px 32px;
            background: linear-gradient(135deg, #4CAF50 0%, #45a049 100%);
            color: #ffffff !important;
            text-decoration: none;
            border-radius: 8px;
            font-weight: 600;
            font-size: 16px;
            box-shadow: 0 4px 12px rgba(76, 175, 80, 0.3);
            transition: all 0.3s ease;
            margin: 20px 0;
        }}
        .cta-button:hover {{
            transform: translateY(-2px);
            box-shadow: 0 6px 16px rgba(76, 175, 80, 0.4);
        }}
        .security-notice {{
            background-color: #fff3e0;
            border-left: 4px solid #ff9800;
            padding: 16px;
            margin: 25px 0;
            border-radius: 4px;
        }}
        .security-icon {{
            font-size: 18px;
            margin-right: 8px;
        }}
        .expiry-info {{
            background-color: #e8f5e8;
            border: 1px solid #4CAF50;
            padding: 16px;
            border-radius: 8px;
            margin: 20px 0;
            text-align: center;
        }}
        .footer {{
            background-color: #f8f9fa;
            padding: 30px 20px;
            text-align: center;
            color: #666666;
            font-size: 14px;
            border-top: 1px solid #e9ecef;
        }}
        .footer-links {{
            margin: 15px 0;
        }}
        .footer-links a {{
            color: #4CAF50;
            text-decoration: none;
            margin: 0 10px;
        }}
        .social-icons {{
            font-size: 20px;
            margin: 15px 0;
        }}
        .social-icons span {{
            margin: 0 8px;
        }}
        @media (max-width: 600px) {{
            .container {{
                margin: 10px;
                border-radius: 8px;
            }}
            .content {{
                padding: 25px 20px;
            }}
            .cta-button {{
                display: block;
                text-align: center;
                margin: 20px 0;
            }}
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <div class='logo'>
                <span class='logo-icon'>💰</span>
                <span>SpendSmart</span>
            </div>
            <p class='tagline'>Smart Financial Management</p>
        </div>
        
        <div class='content'>
            <div class='greeting'>
                👋 Hello {userName}!
            </div>
            
            <div class='message'>
                You requested to change your email address for your SpendSmart account. We're excited to keep you connected with your financial journey! 
            </div>
            
            <div class='message'>
                To complete this change, please verify your new email address by clicking the button below:
            </div>
            
            <div style='text-align: center;'>
                <a href='{verificationLink}' class='cta-button'>
                    ✅ Verify New Email Address
                </a>
            </div>
            
            <div class='expiry-info'>
                <strong>⏰ Time Sensitive:</strong> This verification link will expire in <strong>24 hours</strong>
            </div>
            
            <div class='security-notice'>
                <span class='security-icon'>🔒</span>
                <strong>Security Notice:</strong> If you didn't request this email change, please ignore this message and your account will remain secure. No changes will be made without verification.
            </div>
            
            <div class='message' style='margin-top: 30px; font-size: 14px; color: #888888;'>
                Having trouble with the button? Copy and paste this link into your browser:<br>
                <span style='word-break: break-all; color: #4CAF50;'>{verificationLink}</span>
            </div>
        </div>
        
        <div class='footer'>
            <div class='social-icons'>
                <span>💼</span>
                <span>📊</span>
                <span>💳</span>
                <span>📈</span>
            </div>
            
            <p><strong>SpendSmart Team</strong></p>
            <p>Making financial management simple and smart</p>
            
            <div class='footer-links'>
                <a href='#'>📱 Download App</a>
                <a href='#'>🛡️ Security</a>
                <a href='#'>💬 Support</a>
                <a href='#'>📧 Contact</a>
            </div>
            
            <p style='margin-top: 20px; font-size: 12px; color: #999999;'>
                © 2025 SpendSmart. All rights reserved.<br>
                This is an automated message, please do not reply to this email.
            </p>
        </div>
    </div>
</body>
</html>";

            await SendEmailAsync(newEmail, subject, body);
        }
    }
}

using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using SpendSmart_Backend.Data;
using SpendSmart_Backend.Models;
using Microsoft.EntityFrameworkCore;
using SpendSmart_Backend.DTOs;


namespace SpendSmart_Backend.Services
{
    public class AuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService;

        public AuthService(ApplicationDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }


        public async Task<bool> RegisterUser(RegisterDto dto)
        {
            if (dto.Password != dto.ConfirmPassword)
                return false;

            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
                return false;

            var user = new User
            {
                UserName = dto.UserName,
                Email = dto.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Currency = dto.Currency,
                IsEmailVerified = false,
                EmailVerificationToken = Guid.NewGuid().ToString() // ✅ moved here
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // ✅ Send email after saving
            await _emailService.SendVerificationEmailAsync(user.Email, user.EmailVerificationToken, user.UserName);

            return true;
        }




        public async Task<bool> RegisterAdmin(RegisterDto dto)
        {
            if (dto.Password != dto.ConfirmPassword)
                return false;

            if (await _context.Admins.AnyAsync(a => a.Email == dto.Email))
                return false;

            // Generate token before saving
            var verificationToken = Guid.NewGuid().ToString();

            var admin = new Admin
            {
                UserName = dto.UserName,
                Email = dto.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                EmailVerificationToken = verificationToken
            };

            _context.Admins.Add(admin);
            await _context.SaveChangesAsync();

            await _emailService.SendAdminVerificationEmailAsync(admin.Email, verificationToken, admin.UserName);
            return true;
        }


        public async Task<string> LoginUser(LoginDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null)
                throw new Exception("User not found");

            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(dto.Password, user.Password);

            if (!isPasswordValid)
                throw new Exception("Incorrect password");

            if (!user.IsEmailVerified)
                throw new Exception("Please verify your email before logging in.");

            var token = $"fake-jwt-token-for-{user.UserName}";
            return token;
        }


        public async Task<string> LoginAdmin(LoginDto dto)
        {
            var admin = await _context.Admins.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (admin == null)
                throw new Exception("User not found");


            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(dto.Password, admin.Password);

            if (!isPasswordValid)
                throw new Exception("Incorrect password");

            var token = $"fake-jwt-token-for-{admin.UserName}";
            return token;
        }



        public string GenerateJwtToken(string username, string role)
        {
            var jwtSettings = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("Jwt");

            var claims = new[]
            {
        new Claim(ClaimTypes.Name, username),
        new Claim(ClaimTypes.Role, role)
    };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtSettings["ExpireMinutes"])),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<bool> GenerateResetTokenAsync(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return false;

            user.ResetToken = Guid.NewGuid().ToString();
            user.ResetTokenExpiry = DateTime.UtcNow.AddHours(1);

            await _context.SaveChangesAsync();

            var resetLink = $"http://localhost:5173/resetpassword?token={user.ResetToken}";
            var body = $"Click <a href='{resetLink}'>here</a> to reset your password. This link will expire in 1 hour.";

            await _emailService.SendEmailAsync(user.Email, "Password Reset Request", body);

            return true;
        }


        public async Task<bool> ResetPasswordAsync(string token, string newPassword)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.ResetToken == token && u.ResetTokenExpiry > DateTime.UtcNow);
            if (user == null) return false;

            user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.ResetToken = null;
            user.ResetTokenExpiry = null;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ValidateResetTokenAsync(string token)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.ResetToken == token && u.ResetTokenExpiry > DateTime.UtcNow);
            return user != null;
        }

        public async Task<bool> GenerateAdminResetTokenAsync(string email)
        {
            var admin = await _context.Admins.FirstOrDefaultAsync(a => a.Email == email);
            if (admin == null) return false;

            admin.ResetToken = Guid.NewGuid().ToString();
            admin.ResetTokenExpiry = DateTime.UtcNow.AddHours(1);

            await _context.SaveChangesAsync();

            var resetLink = $"http://localhost:5173/admin/resetpassword?token={admin.ResetToken}";
            var body = $"Click <a href='{resetLink}'>here</a> to reset your password. This link will expire in 1 hour.";

            await _emailService.SendEmailAsync(admin.Email, "Password Reset Request", body);

            return true;
        }


        public async Task<bool> ResetAdminPasswordAsync(string token, string newPassword)
        {
            var admin = await _context.Admins.FirstOrDefaultAsync(a => a.ResetToken == token && a.ResetTokenExpiry > DateTime.UtcNow);
            if (admin == null) return false;

            admin.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
            admin.ResetToken = null;
            admin.ResetTokenExpiry = null;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ValidateAdminResetTokenAsync(string token)
        {
            var admin = await _context.Admins.FirstOrDefaultAsync(a => a.ResetToken == token && a.ResetTokenExpiry > DateTime.UtcNow);
            return admin != null;
        }

        public async Task<ChangePasswordResponseDto> ChangeUserPasswordAsync(ChangePasswordDto request)
        {
            try
            {
                Console.WriteLine($"🔐 DEBUG: Password change request for user {request.UserId}");
                Console.WriteLine($"🔐 DEBUG: Current password provided: {!string.IsNullOrWhiteSpace(request.CurrentPassword)}");
                Console.WriteLine($"🔐 DEBUG: New password provided: {!string.IsNullOrWhiteSpace(request.NewPassword)}");
                Console.WriteLine($"🔐 DEBUG: Confirm password provided: {!string.IsNullOrWhiteSpace(request.ConfirmPassword)}");

                // Validate input
                if (string.IsNullOrWhiteSpace(request.CurrentPassword) ||
                    string.IsNullOrWhiteSpace(request.NewPassword) ||
                    string.IsNullOrWhiteSpace(request.ConfirmPassword))
                {
                    Console.WriteLine("🔐 DEBUG: Validation failed - empty fields");
                    return new ChangePasswordResponseDto { Success = false, Message = "All password fields are required." };
                }

                if (request.NewPassword != request.ConfirmPassword)
                {
                    Console.WriteLine("🔐 DEBUG: Password confirmation mismatch");
                    return new ChangePasswordResponseDto { Success = false, Message = "New password and confirmation do not match." };
                }

                if (request.NewPassword.Length < 6)
                {
                    Console.WriteLine("🔐 DEBUG: Password too short");
                    return new ChangePasswordResponseDto { Success = false, Message = "New password must be at least 6 characters long." };
                }

                // Get user from database
                var user = await _context.Users.FindAsync(request.UserId);
                if (user == null)
                {
                    Console.WriteLine($"🔐 DEBUG: User {request.UserId} not found in database");
                    return new ChangePasswordResponseDto { Success = false, Message = "User not found." };
                }

                Console.WriteLine($"🔐 DEBUG: User found: {user.Email}");
                Console.WriteLine($"🔐 DEBUG: User has password hash: {!string.IsNullOrEmpty(user.Password)}");
                Console.WriteLine($"🔐 DEBUG: Password hash length: {user.Password?.Length ?? 0}");

                // Verify current password with proper error handling
                bool isCurrentPasswordValid = false;
                try
                {
                    isCurrentPasswordValid = BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.Password);
                    Console.WriteLine($"🔐 DEBUG: BCrypt verification result: {isCurrentPasswordValid}");
                }
                catch (Exception bcryptEx)
                {
                    Console.WriteLine($"🔐 DEBUG: BCrypt verification failed with exception: {bcryptEx.Message}");
                    Console.WriteLine($"🔐 DEBUG: Exception type: {bcryptEx.GetType().Name}");
                    // If the stored password hash is corrupted, we need to handle it gracefully
                    return new ChangePasswordResponseDto
                    {
                        Success = false,
                        Message = "Your password data is corrupted. Please use the forgot password feature to reset your password."
                    };
                }

                if (!isCurrentPasswordValid)
                {
                    Console.WriteLine("🔐 DEBUG: Current password verification failed - password incorrect");
                    return new ChangePasswordResponseDto { Success = false, Message = "Current password is incorrect." };
                }

                // Check if new password is same as current (with error handling)
                try
                {
                    if (BCrypt.Net.BCrypt.Verify(request.NewPassword, user.Password))
                    {
                        Console.WriteLine("🔐 DEBUG: New password same as current password");
                        return new ChangePasswordResponseDto { Success = false, Message = "New password must be different from current password." };
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"🔐 DEBUG: Error comparing new password with current: {ex.Message}");
                    // Continue anyway since we already verified the current password above
                }

                // Hash and update password
                user.Password = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
                await _context.SaveChangesAsync();

                Console.WriteLine("🔐 DEBUG: Password changed successfully");
                return new ChangePasswordResponseDto { Success = true, Message = "Password changed successfully." };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"🔐 DEBUG: Exception occurred: {ex.Message}");
                Console.WriteLine($"🔐 DEBUG: Exception type: {ex.GetType().Name}");
                Console.WriteLine($"🔐 DEBUG: Stack trace: {ex.StackTrace}");

                // Return more specific error message based on exception type
                if (ex is DbUpdateException)
                {
                    return new ChangePasswordResponseDto { Success = false, Message = "Database error occurred while updating password." };
                }
                else if (ex is ArgumentException)
                {
                    return new ChangePasswordResponseDto { Success = false, Message = "Invalid password format." };
                }
                else
                {
                    return new ChangePasswordResponseDto { Success = false, Message = $"An error occurred while changing password: {ex.Message}" };
                }
            }
        }

    }

}

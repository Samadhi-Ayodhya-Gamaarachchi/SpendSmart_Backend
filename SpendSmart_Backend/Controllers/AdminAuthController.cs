using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SpendSmart_Backend.Data;
using SpendSmart_Backend.DTOs;
using SpendSmart_Backend.Services;

namespace SpendSmart_Backend.Controllers
{
    [ApiController]
    [Route("api/admin/auth")]
    public class AdminAuthController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly ApplicationDbContext _context;

        public AdminAuthController(AuthService authService, ApplicationDbContext context)
        {
            _authService = authService;
            _context = context;
        }

        // POST: /api/admin/auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            var success = await _authService.RegisterAdmin(dto);
            if (!success)
                return BadRequest(new { message = "Registration failed" });

            return Ok(new { message = "Admin Registration successful! Please check your email to verify" });
        }

        // POST: /api/admin/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            try
            {
                var token = await _authService.LoginAdmin(dto);
                return Ok(new { token });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // POST: /api/admin/auth/forgot-password
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            var result = await _authService.GenerateAdminResetTokenAsync(dto.Email);
            if (!result)
                return BadRequest(new { message = "Email not found" });

            return Ok(new { message = "Password reset link has been sent to your email." });
        }

        // POST: /api/admin/auth/reset-password
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            var result = await _authService.ResetAdminPasswordAsync(dto.Token, dto.NewPassword);
            if (!result)
                return BadRequest(new { message = "Invalid or expired token" });

            return Ok(new { message = "Password has been reset successfully." });
        }

        // POST: /api/admin/auth/validate-reset-token
        [HttpPost("validate-reset-token")]
        public async Task<IActionResult> ValidateResetToken([FromBody] TokenDto dto)
        {
            var isValid = await _authService.ValidateAdminResetTokenAsync(dto.Token);
            return isValid ? Ok() : BadRequest(new { message = "Invalid or expired token" });
        }

        // GET: /api/admin/auth/verify-email?email=xxx&token=yyy
        [HttpGet("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromQuery] string email, [FromQuery] string token)
        {
            var admin = await _context.Admins.FirstOrDefaultAsync(a => a.Email == email);

            if (admin == null)
                return BadRequest(new { success = false, message = "User not found." });

            if (admin.IsEmailVerified)
                return Ok(new { success = true, message = "Email is already verified!" });

            if (admin.EmailVerificationToken != token)
                return BadRequest(new { success = false, message = "Invalid verification link." });

            admin.IsEmailVerified = true;
            admin.EmailVerificationToken = null;
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Email successfully verified!" });
        }

    }
}

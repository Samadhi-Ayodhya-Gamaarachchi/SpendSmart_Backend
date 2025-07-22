using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SpendSmart_Backend.Data;
using SpendSmart_Backend.DTOs;
using SpendSmart_Backend.Services;

namespace SpendSmart_Backend.Controllers
{
    [ApiController]
    [Route("api/user/auth")]
    public class UserAuthController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly ApplicationDbContext _context; // ← Add this

        public UserAuthController(AuthService authService, ApplicationDbContext context)
        {
            _authService = authService;
            _context = context; // ← Assign here
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            var success = await _authService.RegisterUser(dto);
            if (!success) return BadRequest("Registration failed");
            return Ok(new { message = "User registration successful! please check your email to verify" });

        }

        
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            try
            {
                var token = await _authService.LoginUser(dto);
                return Ok(new { token });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }

        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            var result = await _authService.GenerateResetTokenAsync(dto.Email);
            if (!result)
                return BadRequest(new { message = "Email not found" });

            return Ok(new { message = "Password reset link has been sent to your email." });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            var result = await _authService.ResetPasswordAsync(dto.Token, dto.NewPassword);
            if (!result)
                return BadRequest(new { message = "Invalid or expired token" });

            return Ok(new { message = "Password has been reset successfully." });
        }

        [HttpPost("validate-reset-token")]
        public async Task<IActionResult> ValidateResetToken([FromBody] TokenDto dto)
        {
            var isValid = await _authService.ValidateResetTokenAsync(dto.Token);
            return isValid ? Ok() : BadRequest(new { message = "Invalid or expired token" });
        }

        [HttpGet("verify-email")]
        public async Task<IActionResult> VerifyEmail(string email, string token)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
                return BadRequest(new { success = false, message = "User not found." });

            
            if (user.IsEmailVerified)
                return Ok(new { success = true, message = "Email is already verified!" });

            
            if (user.EmailVerificationToken != token)
                return BadRequest(new { success = false, message = "Invalid verification link." });

            user.IsEmailVerified = true;
            user.EmailVerificationToken = null;
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Email successfully verified!" });
        }









    }

}

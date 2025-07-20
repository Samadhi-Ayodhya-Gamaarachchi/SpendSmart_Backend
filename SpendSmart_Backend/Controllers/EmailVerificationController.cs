using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SpendSmart_Backend.Data;
using SpendSmart_Backend.DTOs;
using SpendSmart_Backend.Services;
using System.Security.Claims;

namespace SpendSmart_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous] // Allow anonymous access for testing without JWT authentication
    public class EmailVerificationController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService;

        public EmailVerificationController(ApplicationDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok(new { message = "EmailVerificationController is working!" });
        }

        [HttpPost("request-change")]
        public async Task<IActionResult> RequestEmailChange([FromBody] EmailChangeRequestDto request)
        {
            try
            {
                // TODO: Replace with proper JWT authentication
                // For now, use UserId from request body for testing
                int userId = request.UserId;

                if (userId <= 0)
                {
                    return BadRequest(new EmailChangeResponseDto
                    {
                        Success = false,
                        Message = "Invalid user ID"
                    });
                }

                // Find the user
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (user == null)
                {
                    return NotFound(new EmailChangeResponseDto
                    {
                        Success = false,
                        Message = "User not found"
                    });
                }

                // Check if the new email is the same as current email
                if (user.Email.Equals(request.NewEmail, StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest(new EmailChangeResponseDto
                    {
                        Success = false,
                        Message = "New email cannot be the same as current email"
                    });
                }

                // Check if the new email is already in use by another user
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == request.NewEmail && u.Id != userId);

                if (existingUser != null)
                {
                    return BadRequest(new EmailChangeResponseDto
                    {
                        Success = false,
                        Message = "Email address is already in use"
                    });
                }

                // Check if there's a pending email change to the same email
                var pendingChange = await _context.Users
                    .FirstOrDefaultAsync(u => u.PendingEmail == request.NewEmail && u.Id != userId);

                if (pendingChange != null)
                {
                    return BadRequest(new EmailChangeResponseDto
                    {
                        Success = false,
                        Message = "This email address is already pending verification by another user"
                    });
                }

                // Generate verification token and set expiry (24 hours)
                var token = Guid.NewGuid().ToString();
                var expiry = DateTime.UtcNow.AddHours(24);

                // Update user with pending email change details
                user.PendingEmail = request.NewEmail;
                user.EmailChangeToken = token;
                user.EmailChangeTokenExpiry = expiry;

                await _context.SaveChangesAsync();

                // Send verification email to the new email address
                await _emailService.SendEmailChangeVerificationAsync(
                    request.NewEmail,
                    token,
                    user.UserName,
                    userId
                );

                return Ok(new EmailChangeResponseDto
                {
                    Success = true,
                    Message = "Verification email sent to new email address. Please check your inbox and click the verification link."
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new EmailChangeResponseDto
                {
                    Success = false,
                    Message = "An error occurred while processing your request"
                });
            }
        }

        [HttpGet("verify-change")]
        public async Task<IActionResult> VerifyEmailChange([FromQuery] int userId, [FromQuery] string token)
        {
            try
            {
                // Find the user with the pending email change
                var user = await _context.Users.FirstOrDefaultAsync(u =>
                    u.Id == userId &&
                    u.EmailChangeToken == token &&
                    u.EmailChangeTokenExpiry > DateTime.UtcNow);

                if (user == null)
                {
                    return BadRequest(new EmailChangeResponseDto
                    {
                        Success = false,
                        Message = "Invalid or expired verification link"
                    });
                }

                // Check if pending email is still available
                var emailTaken = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == user.PendingEmail && u.Id != userId);

                if (emailTaken != null)
                {
                    // Clear the pending change since email is now taken
                    user.PendingEmail = null;
                    user.EmailChangeToken = null;
                    user.EmailChangeTokenExpiry = null;
                    await _context.SaveChangesAsync();

                    return BadRequest(new EmailChangeResponseDto
                    {
                        Success = false,
                        Message = "Email address is no longer available"
                    });
                }

                // Update the user's email
                user.Email = user.PendingEmail!; // We know it's not null from the check above
                user.PendingEmail = null;
                user.EmailChangeToken = null;
                user.EmailChangeTokenExpiry = null;

                await _context.SaveChangesAsync();

                // Redirect to frontend success page
                return Redirect("http://localhost:5173/settings?emailChanged=true");
            }
            catch (Exception)
            {
                // Redirect to frontend error page
                return Redirect("http://localhost:5173/settings?emailChanged=false&error=true");
            }
        }

        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromBody] EmailVerificationDto request)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u =>
                    u.Id == request.UserId &&
                    u.EmailVerificationToken == request.Token);

                if (user == null)
                {
                    return BadRequest(new { success = false, message = "Invalid verification token" });
                }

                user.IsEmailVerified = true;
                user.EmailVerificationToken = null;

                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Email verified successfully" });
            }
            catch (Exception)
            {
                return StatusCode(500, new { success = false, message = "An error occurred during verification" });
            }
        }

        [HttpPost("cancel-change")]
        public async Task<IActionResult> CancelEmailChange([FromBody] EmailVerificationDto request)
        {
            try
            {
                // TODO: Replace with proper JWT authentication
                // For now, use UserId from request body for testing
                int userId = request.UserId;

                if (userId <= 0)
                {
                    return BadRequest(new EmailChangeResponseDto
                    {
                        Success = false,
                        Message = "Invalid user ID"
                    });
                }

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (user == null)
                {
                    return NotFound(new EmailChangeResponseDto
                    {
                        Success = false,
                        Message = "User not found"
                    });
                }

                // Clear pending email change
                user.PendingEmail = null;
                user.EmailChangeToken = null;
                user.EmailChangeTokenExpiry = null;

                await _context.SaveChangesAsync();

                return Ok(new EmailChangeResponseDto
                {
                    Success = true,
                    Message = "Email change request cancelled successfully"
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new EmailChangeResponseDto
                {
                    Success = false,
                    Message = "An error occurred while cancelling the request"
                });
            }
        }

        [HttpGet("pending-change")]
        public async Task<IActionResult> GetPendingEmailChange([FromQuery] int userId)
        {
            try
            {
                // TODO: Replace with proper JWT authentication
                // For now, use UserId from query parameter for testing
                if (userId <= 0)
                {
                    return BadRequest(new { success = false, message = "Invalid user ID" });
                }

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (user == null)
                {
                    return NotFound(new { success = false, message = "User not found" });
                }

                if (string.IsNullOrEmpty(user.PendingEmail) ||
                    user.EmailChangeTokenExpiry < DateTime.UtcNow)
                {
                    // Clean up expired pending changes
                    if (!string.IsNullOrEmpty(user.PendingEmail))
                    {
                        user.PendingEmail = null;
                        user.EmailChangeToken = null;
                        user.EmailChangeTokenExpiry = null;
                        await _context.SaveChangesAsync();
                    }

                    return Ok(new
                    {
                        success = true,
                        hasPendingChange = false,
                        currentEmail = user.Email
                    });
                }

                return Ok(new
                {
                    success = true,
                    hasPendingChange = true,
                    currentEmail = user.Email,
                    pendingEmail = user.PendingEmail,
                    expiresAt = user.EmailChangeTokenExpiry
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }
    }
}
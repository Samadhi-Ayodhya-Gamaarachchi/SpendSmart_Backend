using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SpendSmart_Backend.Data;
using SpendSmart_Backend.Models;
using SpendSmart_Backend.Services;
using SpendSmart_Backend.DTOs;
using System.ComponentModel.DataAnnotations;

namespace SpendSmart_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UserController> _logger;
        private readonly AuthService _authService;

        public UserController(ApplicationDbContext context, ILogger<UserController> logger, AuthService authService)
        {
            _context = context;
            _logger = logger;
            _authService = authService;
        }

        /// <summary>
        /// Update user name
        /// </summary>
        /// <param name="request">Update name request</param>
        /// <returns>Update result</returns>
        [HttpPut("update-name")]
        public async Task<IActionResult> UpdateUserName([FromBody] UpdateUserNameRequest request)
        {
            try
            {
                _logger.LogInformation($"Update name request received for user {request.UserId}");

                if (request.UserId <= 0)
                {
                    return BadRequest(new { success = false, message = "Invalid user ID" });
                }

                if (string.IsNullOrWhiteSpace(request.UserName))
                {
                    return BadRequest(new { success = false, message = "Name cannot be empty" });
                }

                var user = await _context.Users.FindAsync(request.UserId);
                if (user == null)
                {
                    return NotFound(new { success = false, message = "User not found" });
                }

                // Update user name (you might need to split into FirstName/LastName)
                var nameParts = request.UserName.Trim().Split(' ', 2);
                user.UserName = request.UserName;
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation($"User name updated successfully for user {request.UserId}");

                return Ok(new
                {
                    success = true,
                    message = "Name updated successfully!",
                    firstName = user.FirstName,
                    lastName = user.LastName,
                    fullName = $"{user.FirstName} {user.LastName}".Trim()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user name");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Update user email (now requires verification)
        /// </summary>
        /// <param name="request">Update email request</param>
        /// <returns>Update result</returns>
        [HttpPut("update-email")]
        [Obsolete("Use EmailVerification/request-change endpoint instead")]
        public IActionResult UpdateUserEmail([FromBody] UpdateUserEmailRequest request)
        {
            return BadRequest(new
            {
                success = false,
                message = "Email updates now require verification. Please use the /api/EmailVerification/request-change endpoint instead."
            });
        }

        /// <summary>
        /// Get user profile
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>User profile</returns>
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUser(int userId)
        {
            try
            {
                if (userId <= 0)
                {
                    return BadRequest(new { message = "Invalid user ID" });
                }

                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                return Ok(new
                {
                    id = user.Id,
                    userName = user.UserName,
                    firstName = user.FirstName,
                    lastName = user.LastName,
                    fullName = $"{user.FirstName} {user.LastName}".Trim(),
                    email = user.Email,
                    currency = user.Currency,
                    profilePictureUrl = user.ProfilePictureUrl,
                    createdAt = user.CreatedAt,
                    updatedAt = user.UpdatedAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Change user password
        /// </summary>
        /// <param name="request">Password change request</param>
        /// <returns>Password change result</returns>
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto request)
        {
            try
            {
                _logger.LogInformation($"Password change request received for user {request.UserId}");

                var result = await _authService.ChangeUserPasswordAsync(request);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password");
                return StatusCode(500, new ChangePasswordResponseDto 
                { 
                    Success = false, 
                    Message = "Internal server error occurred while changing password" 
                });
            }
        }
    }

    // Request DTOs
    public class UpdateUserNameRequest
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        [StringLength(200, MinimumLength = 1)]
        public string UserName { get; set; } = string.Empty;
    }

    public class UpdateUserEmailRequest
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; } = string.Empty;
    }
}
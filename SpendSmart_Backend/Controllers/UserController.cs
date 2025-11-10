using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SpendSmart_Backend.Data;
using SpendSmart_Backend.DTOs;
using SpendSmart_Backend.Models;

namespace SpendSmart_Backend.Controllers
{
    [Route("api/[controller]")]
    public class UserController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UserController> _logger;

        public UserController(ApplicationDbContext context, ILogger<UserController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get current user's profile information
        /// </summary>
        [HttpGet("profile")]
        public async Task<IActionResult> GetUserProfile()
        {
            try
            {
                var userId = GetCurrentUserId();
                var user = await _context.Users
                    .Where(u => u.Id == userId)
                    .Select(u => new
                    {
                        u.Id,
                        u.UserName,
                        u.Email,
                        u.IsEmailVerified
                    })
                    .FirstOrDefaultAsync();

                if (user == null)
                {
                    return NotFound("User not found");
                }

                return Ok(user);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized("Invalid token or user access");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user profile");
                return StatusCode(500, "An error occurred while getting user profile");
            }
        }

        /// <summary>
        /// Update current user's profile information
        /// </summary>
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateUserProfile([FromBody] UpdateUserProfileDto updateDto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var user = await _context.Users.FindAsync(userId);

                if (user == null)
                {
                    return NotFound("User not found");
                }

                // Update only provided fields
                if (!string.IsNullOrEmpty(updateDto.UserName))
                {
                    user.UserName = updateDto.UserName;
                }

                if (!string.IsNullOrEmpty(updateDto.Email))
                {
                    // Check if email is already taken by another user
                    var emailExists = await _context.Users
                        .AnyAsync(u => u.Email == updateDto.Email && u.Id != userId);
                    
                    if (emailExists)
                    {
                        return BadRequest("Email is already taken");
                    }
                    
                    user.Email = updateDto.Email;
                    user.IsEmailVerified = false; // Reset email verification if email changed
                }

                await _context.SaveChangesAsync();

                return Ok(new { message = "Profile updated successfully" });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized("Invalid token or user access");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user profile");
                return StatusCode(500, "An error occurred while updating user profile");
            }
        }

        /// <summary>
        /// Get current user's statistics
        /// </summary>
        [HttpGet("stats")]
        public async Task<IActionResult> GetUserStats()
        {
            try
            {
                var userId = GetCurrentUserId();
                
                var stats = new
                {
                    TransactionCount = await _context.Transactions.CountAsync(t => t.UserId == userId),
                    GoalCount = await _context.Goals.CountAsync(g => g.UserId == userId),
                    BudgetCount = await _context.Budgets.CountAsync(b => b.UserId == userId),
                    TotalIncome = await _context.Transactions
                        .Where(t => t.UserId == userId && t.Type == "Income")
                        .SumAsync(t => t.Amount),
                    TotalExpense = await _context.Transactions
                        .Where(t => t.UserId == userId && t.Type == "Expense")
                        .SumAsync(t => t.Amount)
                };

                return Ok(stats);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized("Invalid token or user access");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user stats");
                return StatusCode(500, "An error occurred while getting user stats");
            }
        }
    }
}

// Add this DTO class for the update profile endpoint
namespace SpendSmart_Backend.DTOs
{
    public class UpdateUserProfileDto
    {
        public string? UserName { get; set; }
        public string? Email { get; set; }
    }
}
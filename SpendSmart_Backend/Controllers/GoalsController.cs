using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SpendSmart_Backend.Data;
using SpendSmart_Backend.DTOs;
using SpendSmart_Backend.Models;
using Microsoft.AspNetCore.Authorization;

namespace SpendSmart_Backend.Controllers
{
    [Route("api/[controller]")]
    public class GoalsController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<GoalsController> _logger;

        public GoalsController(ApplicationDbContext context, ILogger<GoalsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/Goals - Get current user's goals only
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<GoalDto>>> GetGoals()
        {
            try
            {
                // Get user ID from JWT token - users can only see their own goals
                var userId = GetCurrentUserId();
                _logger.LogInformation("Getting goals for user {UserId}", userId);

                var goals = await _context.Goals
                    .Where(g => g.UserId == userId)
                    .ToListAsync();

                var goalDtos = goals.Select(goal => new GoalDto
                {
                    Id = goal.Id,
                    Name = goal.Name,
                    TargetAmount = goal.TargetAmount,
                    CurrentAmount = goal.CurrentAmount,
                    StartDate = goal.StartDate,
                    EndDate = goal.EndDate,
                    Description = goal.Description ?? "",
                    UserId = goal.UserId,
                    Progress = goal.TargetAmount > 0 ? Math.Round((goal.CurrentAmount / goal.TargetAmount) * 100, 2) : 0,
                    RemainingDays = (int)(goal.EndDate - DateTime.Now).TotalDays
                }).ToList();

                _logger.LogInformation("Returning {Count} goals for user {UserId}", goalDtos.Count, userId);
                return Ok(goalDtos);
            }
            catch(UnauthorizedAccessException)
            {
                return Unauthorized("Invalid token or user access");
            }
        }

        // This endpoint is now redundant since GetGoals() already filters by current user
        // Keeping it for backward compatibility but it will return the same data as GetGoals()
        [HttpGet("user")]
        public async Task<ActionResult<IEnumerable<GoalDto>>> GetGoalsByUser()
        {
            // Redirect to the main GetGoals method which now handles user filtering
            return await GetGoals();
        }


        // POST: api/Goals
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<GoalDto>> CreateGoal(CreateGoalDto createGoalDto)
        {
            try
            {
                // Get user ID from JWT token - users can only create goals for themselves
                var userId = GetCurrentUserId();
                _logger.LogInformation("Creating new goal for user {UserId}: {@Goal}", userId, createGoalDto);

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state: {@Errors}", ModelState);
                    return BadRequest(ModelState);
                }

                // Override the UserId from DTO with the authenticated user's ID
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return BadRequest($"User with ID {userId} not found");
                }

                var goal = new Goal
                {
                    Name = createGoalDto.Name,
                    TargetAmount = createGoalDto.TargetAmount,
                    CurrentAmount = createGoalDto.CurrentAmount,
                    StartDate = createGoalDto.StartDate,
                    EndDate = createGoalDto.EndDate,
                    Description = createGoalDto.Description ?? string.Empty,
                    UserId = userId, // Use authenticated user's ID, not from DTO
                    User = user
                };

                _context.Goals.Add(goal);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Goal created successfully with ID: {GoalId} for user {UserId}", goal.Id, userId);

                var goalDto = new GoalDto
                {
                    Id = goal.Id,
                    Name = goal.Name,
                    TargetAmount = goal.TargetAmount,
                    CurrentAmount = goal.CurrentAmount,
                    StartDate = goal.StartDate,
                    EndDate = goal.EndDate,
                    Description = goal.Description ?? "",
                    UserId = goal.UserId,
                    Progress = goal.TargetAmount > 0 ? Math.Round((goal.CurrentAmount / goal.TargetAmount) * 100, 2) : 0,
                    RemainingDays = (int)(goal.EndDate - DateTime.Now).TotalDays
                };

                return CreatedAtAction(nameof(GetGoals), goalDto);
            }
            catch(UnauthorizedAccessException)
            {
                return Unauthorized("Invalid token or user access");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating goal");
                return StatusCode(500, "An error occurred while creating the goal");
            }
        }

        // PUT: api/Goals/5
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<GoalDto>> UpdateGoal(int id, [FromBody] UpdateGoalDto updateGoalDto)
        {
            try
            {
                // Get user ID from JWT token - users can only update their own goals
                var userId = GetCurrentUserId();
                _logger.LogInformation("Updating goal with ID: {GoalId} for user {UserId}, Data: {@Goal}", id, userId, updateGoalDto);

                // Find goal and verify ownership
                var goal = await _context.Goals.FirstOrDefaultAsync(g => g.Id == id && g.UserId == userId);
                if (goal == null)
                {
                    _logger.LogWarning("Goal with ID {GoalId} not found for user {UserId}", id, userId);
                    return NotFound("Goal not found or you don't have permission to update it");
                }

                // Update properties if they are provided
                if (updateGoalDto.Name != null)
                    goal.Name = updateGoalDto.Name;

                if (updateGoalDto.TargetAmount.HasValue)
                    goal.TargetAmount = updateGoalDto.TargetAmount.Value;

                if (updateGoalDto.CurrentAmount.HasValue)
                    goal.CurrentAmount = updateGoalDto.CurrentAmount.Value;

                if (updateGoalDto.StartDate.HasValue)
                    goal.StartDate = updateGoalDto.StartDate.Value;

                if (updateGoalDto.EndDate.HasValue)
                    goal.EndDate = updateGoalDto.EndDate.Value;

                if (updateGoalDto.Description != null)
                    goal.Description = updateGoalDto.Description;

                _context.Entry(goal).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Goal updated successfully for user {UserId}", userId);

                // Return the updated goal
                var goalDto = new GoalDto
                {
                    Id = goal.Id,
                    Name = goal.Name,
                    TargetAmount = goal.TargetAmount,
                    CurrentAmount = goal.CurrentAmount,
                    StartDate = goal.StartDate,
                    EndDate = goal.EndDate,
                    Description = goal.Description ?? "",
                    UserId = goal.UserId,
                    Progress = goal.TargetAmount > 0 ? Math.Round((goal.CurrentAmount / goal.TargetAmount) * 100, 2) : 0,
                    RemainingDays = (int)(goal.EndDate - DateTime.Now).TotalDays
                };

                return Ok(goalDto);
            }
            catch(UnauthorizedAccessException)
            {
                return Unauthorized("Invalid token or user access");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating goal with ID: {GoalId}", id);
                return StatusCode(500, "An error occurred while updating the goal");
            }
        }

        // DELETE: api/Goals/5
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteGoal(int id)
        {
            try
            {
                // Get user ID from JWT token - users can only delete their own goals
                var userId = GetCurrentUserId();
                
                // Find goal and verify ownership
                var goal = await _context.Goals.FirstOrDefaultAsync(g => g.Id == id && g.UserId == userId);
                if (goal == null)
                {
                    return NotFound("Goal not found or you don't have permission to delete it");
                }

                _context.Goals.Remove(goal);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch(UnauthorizedAccessException)
            {
                return Unauthorized("Invalid token or user access");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting goal with ID: {GoalId}", id);
                return StatusCode(500, "An error occurred while deleting the goal");
            }
        }
    }
}

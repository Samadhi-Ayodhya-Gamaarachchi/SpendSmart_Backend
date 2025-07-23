using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SpendSmart_Backend.Data;
using SpendSmart_Backend.DTOs;
using SpendSmart_Backend.Models;

namespace SpendSmart_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GoalsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<GoalsController> _logger;

        public GoalsController(ApplicationDbContext context, ILogger<GoalsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/Goals
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<GoalDto>>> GetGoals()
        {
            _logger.LogInformation("Getting all goals");

            var goals = await _context.Goals.ToListAsync();

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

            _logger.LogInformation("Returning {Count} goals", goalDtos.Count);
            return Ok(goalDtos);
        }

        // GET: api/Goals/5
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GoalDto>> GetGoal(int id)
        {
            var goal = await _context.Goals.FindAsync(id);

            if (goal == null)
            {
                return NotFound();
            }

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

            return goalDto;
        }

        // POST: api/Goals
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<GoalDto>> CreateGoal(CreateGoalDto createGoalDto)
        {
            _logger.LogInformation("Creating new goal: {@Goal}", createGoalDto);

            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state: {@Errors}", ModelState);
                    return BadRequest(ModelState);
                }

                var user = await _context.Users.FindAsync(createGoalDto.UserId);
                if (user == null)
                {
                    return BadRequest($"User with ID {createGoalDto.UserId} not found");
                }

                var goal = new Goal
                {
                    Name = createGoalDto.Name,
                    TargetAmount = createGoalDto.TargetAmount,
                    CurrentAmount = createGoalDto.CurrentAmount,
                    StartDate = createGoalDto.StartDate,
                    EndDate = createGoalDto.EndDate,
                    Description = createGoalDto.Description ?? string.Empty,
                    UserId = createGoalDto.UserId,
                    User = user
                };

                _context.Goals.Add(goal);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Goal created successfully with ID: {GoalId}", goal.Id);

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

                return CreatedAtAction(nameof(GetGoal), new { id = goal.Id }, goalDto);
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
            _logger.LogInformation("Updating goal with ID: {GoalId}, Data: {@Goal}", id, updateGoalDto);

            try
            {
                var goal = await _context.Goals.FindAsync(id);
                if (goal == null)
                {
                    _logger.LogWarning("Goal with ID {GoalId} not found", id);
                    return NotFound();
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

                _logger.LogInformation("Goal updated successfully");

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
            var goal = await _context.Goals.FindAsync(id);
            if (goal == null)
            {
                return NotFound();
            }

            try
            {
                _context.Goals.Remove(goal);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting goal with ID: {GoalId}", id);
                return StatusCode(500, "An error occurred while deleting the goal");
            }
        }
    }
}

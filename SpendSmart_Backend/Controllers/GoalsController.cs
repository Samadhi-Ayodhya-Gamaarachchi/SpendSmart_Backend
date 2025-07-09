using Microsoft.AspNetCore.Mvc;
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

        public GoalsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Goals
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GoalDto>>> GetGoals()
        {
            var goals = await _context.Goals
                .Include(g => g.User)
                .Select(g => MapToDto(g))
                .ToListAsync();

            return Ok(goals);
        }

        // GET: api/Goals/5
        [HttpGet("{id}")]
        public async Task<ActionResult<GoalDto>> GetGoal(int id)
        {
            var goal = await _context.Goals
                .Include(g => g.User)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (goal == null)
            {
                return NotFound();
            }

            return Ok(MapToDto(goal));
        }

        // GET: api/Goals/user/5
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<GoalDto>>> GetGoalsByUser(int userId)
        {
            var goals = await _context.Goals
                .Include(g => g.User)
                .Where(g => g.UserId == userId)
                .Select(g => MapToDto(g))
                .ToListAsync();

            return Ok(goals);
        }

        // POST: api/Goals
        [HttpPost]
        public async Task<ActionResult<GoalDto>> CreateGoal(CreateGoalDto createGoalDto)
        {
            try
            {
                // Debug logging
                Console.WriteLine("=== GOAL CREATION DEBUG ===");
                Console.WriteLine($"Received CreateGoalDto:");
                Console.WriteLine($"  Name: {createGoalDto.Name}");
                Console.WriteLine($"  TargetAmount: {createGoalDto.TargetAmount}");
                Console.WriteLine($"  CurrentAmount: {createGoalDto.CurrentAmount}");
                Console.WriteLine($"  StartDate: {createGoalDto.StartDate}");
                Console.WriteLine($"  EndDate: {createGoalDto.EndDate}");
                Console.WriteLine($"  Description: {createGoalDto.Description}");
                Console.WriteLine($"  UserId: {createGoalDto.UserId}");

                if (!ModelState.IsValid)
                {
                    Console.WriteLine("ModelState is invalid:");
                    foreach (var error in ModelState)
                    {
                        Console.WriteLine($"  {error.Key}: {string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
                    }
                    return BadRequest(ModelState);
                }

                // For development - create user if doesn't exist
                var userExists = await _context.Users.AnyAsync(u => u.Id == createGoalDto.UserId);
                if (!userExists)
                {
                    // Create a default user for development
                    var newUser = new User
                    {
                        Id = createGoalDto.UserId,
                        UserName = $"TestUser{createGoalDto.UserId}",
                        Password = "password123", // This should be hashed in production
                        FirstName = "Test",
                        LastName = "User",
                        Email = $"testuser{createGoalDto.UserId}@example.com",
                        Currency = "USD"
                    };

                    _context.Users.Add(newUser);
                    await _context.SaveChangesAsync();
                }

                // Validate dates
                if (createGoalDto.EndDate <= createGoalDto.StartDate)
                {
                    return BadRequest("End date must be after start date");
                }

                var goal = new Goal
                {
                    Name = createGoalDto.Name,
                    TargetAmount = createGoalDto.TargetAmount,
                    CurrentAmount = createGoalDto.CurrentAmount,
                    StartDate = createGoalDto.StartDate,
                    EndDate = createGoalDto.EndDate,
                    Description = createGoalDto.Description ?? string.Empty,
                    UserId = createGoalDto.UserId
                };

                Console.WriteLine($"Creating Goal entity:");
                Console.WriteLine($"  Name: {goal.Name}");
                Console.WriteLine($"  TargetAmount: {goal.TargetAmount}");
                Console.WriteLine($"  CurrentAmount: {goal.CurrentAmount}");
                Console.WriteLine($"  StartDate: {goal.StartDate}");
                Console.WriteLine($"  EndDate: {goal.EndDate}");
                Console.WriteLine($"  Description: {goal.Description}");
                Console.WriteLine($"  UserId: {goal.UserId}");

                _context.Goals.Add(goal);
                await _context.SaveChangesAsync();

                Console.WriteLine($"Goal saved with ID: {goal.Id}");

                // Reload the goal with user information
                var createdGoal = await _context.Goals
                    .Include(g => g.User)
                    .FirstOrDefaultAsync(g => g.Id == goal.Id);

                Console.WriteLine($"Reloaded goal from database:");
                Console.WriteLine($"  Name: {createdGoal!.Name}");
                Console.WriteLine($"  TargetAmount: {createdGoal.TargetAmount}");
                Console.WriteLine($"  CurrentAmount: {createdGoal.CurrentAmount}");
                Console.WriteLine($"  StartDate: {createdGoal.StartDate}");
                Console.WriteLine($"  EndDate: {createdGoal.EndDate}");

                var responseDto = MapToDto(createdGoal);
                Console.WriteLine($"Mapped DTO for response:");
                Console.WriteLine($"  Name: {responseDto.Name}");
                Console.WriteLine($"  TargetAmount: {responseDto.TargetAmount}");
                Console.WriteLine($"  CurrentAmount: {responseDto.CurrentAmount}");
                Console.WriteLine($"  Progress: {responseDto.Progress}");
                Console.WriteLine($"  RemainingDays: {responseDto.RemainingDays}");

                return CreatedAtAction(nameof(GetGoal), new { id = goal.Id }, responseDto);
            }
            catch (Exception ex)
            {
                // Log the error for debugging
                Console.WriteLine($"Error creating goal: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return BadRequest($"Error creating goal: {ex.Message}");
            }
        }

        // PUT: api/Goals/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateGoal(int id, UpdateGoalDto updateGoalDto)
        {
            Console.WriteLine($"=== GOAL UPDATE ===");
            Console.WriteLine($"Updating goal with ID: {id}");
            Console.WriteLine($"Received UpdateGoalDto:");
            Console.WriteLine($"  Name: {updateGoalDto.Name}");
            Console.WriteLine($"  TargetAmount: {updateGoalDto.TargetAmount}");
            Console.WriteLine($"  CurrentAmount: {updateGoalDto.CurrentAmount}");
            Console.WriteLine($"  StartDate: {updateGoalDto.StartDate}");
            Console.WriteLine($"  EndDate: {updateGoalDto.EndDate}");
            Console.WriteLine($"  Description: {updateGoalDto.Description}");

            var goal = await _context.Goals.FindAsync(id);
            if (goal == null)
            {
                Console.WriteLine($"Goal with ID {id} not found");
                return NotFound();
            }

            Console.WriteLine($"Found existing goal: {goal.Name}");
            Console.WriteLine($"  Current TargetAmount: {goal.TargetAmount}");
            Console.WriteLine($"  Current CurrentAmount: {goal.CurrentAmount}");
            Console.WriteLine($"  Current StartDate: {goal.StartDate}");
            Console.WriteLine($"  Current EndDate: {goal.EndDate}");

            // Update only provided fields
            if (!string.IsNullOrEmpty(updateGoalDto.Name))
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

            Console.WriteLine($"Updated goal values:");
            Console.WriteLine($"  Name: {goal.Name}");
            Console.WriteLine($"  TargetAmount: {goal.TargetAmount}");
            Console.WriteLine($"  CurrentAmount: {goal.CurrentAmount}");
            Console.WriteLine($"  StartDate: {goal.StartDate}");
            Console.WriteLine($"  EndDate: {goal.EndDate}");

            // Validate dates if both are provided
            if (goal.EndDate <= goal.StartDate)
            {
                Console.WriteLine("Date validation failed: End date must be after start date");
                return BadRequest("End date must be after start date");
            }

            try
            {
                Console.WriteLine($"Saving goal changes to database...");
                await _context.SaveChangesAsync();
                Console.WriteLine($"Goal {id} updated successfully in database");
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!GoalExists(id))
                {
                    Console.WriteLine($"Goal {id} no longer exists");
                    return NotFound();
                }
                Console.WriteLine($"Concurrency exception updating goal {id}");
                throw;
            }

            return NoContent();
        }

        // PUT: api/Goals/5/full - Full update endpoint
        [HttpPut("{id}/full")]
        public async Task<ActionResult<GoalDto>> UpdateGoalFull(int id, CreateGoalDto updateGoalDto)
        {
            Console.WriteLine($"=== FULL GOAL UPDATE ===");
            Console.WriteLine($"Updating goal with ID: {id}");
            Console.WriteLine($"Received CreateGoalDto for full update:");
            Console.WriteLine($"  Name: {updateGoalDto.Name}");
            Console.WriteLine($"  TargetAmount: {updateGoalDto.TargetAmount}");
            Console.WriteLine($"  CurrentAmount: {updateGoalDto.CurrentAmount}");
            Console.WriteLine($"  StartDate: {updateGoalDto.StartDate}");
            Console.WriteLine($"  EndDate: {updateGoalDto.EndDate}");
            Console.WriteLine($"  Description: {updateGoalDto.Description}");
            Console.WriteLine($"  UserId: {updateGoalDto.UserId}");

            if (!ModelState.IsValid)
            {
                Console.WriteLine("ModelState is invalid:");
                foreach (var error in ModelState)
                {
                    Console.WriteLine($"  {error.Key}: {string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
                }
                return BadRequest(ModelState);
            }

            var goal = await _context.Goals.FindAsync(id);
            if (goal == null)
            {
                Console.WriteLine($"Goal with ID {id} not found");
                return NotFound();
            }

            Console.WriteLine($"Found existing goal: {goal.Name}");

            // Validate dates
            if (updateGoalDto.EndDate <= updateGoalDto.StartDate)
            {
                Console.WriteLine("Date validation failed: End date must be after start date");
                return BadRequest("End date must be after start date");
            }

            // Update all fields
            goal.Name = updateGoalDto.Name;
            goal.TargetAmount = updateGoalDto.TargetAmount;
            goal.CurrentAmount = updateGoalDto.CurrentAmount;
            goal.StartDate = updateGoalDto.StartDate;
            goal.EndDate = updateGoalDto.EndDate;
            goal.Description = updateGoalDto.Description ?? string.Empty;
            // Don't update UserId for security reasons

            Console.WriteLine($"Updated goal values:");
            Console.WriteLine($"  Name: {goal.Name}");
            Console.WriteLine($"  TargetAmount: {goal.TargetAmount}");
            Console.WriteLine($"  CurrentAmount: {goal.CurrentAmount}");
            Console.WriteLine($"  StartDate: {goal.StartDate}");
            Console.WriteLine($"  EndDate: {goal.EndDate}");

            try
            {
                Console.WriteLine($"Saving full goal update to database...");
                await _context.SaveChangesAsync();
                Console.WriteLine($"Goal {id} fully updated successfully in database");

                // Reload the goal with user information
                var updatedGoal = await _context.Goals
                    .Include(g => g.User)
                    .FirstOrDefaultAsync(g => g.Id == goal.Id);

                var responseDto = MapToDto(updatedGoal!);
                Console.WriteLine($"Returning updated goal DTO: {responseDto.Name}");

                return Ok(responseDto);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating goal: {ex.Message}");
                return BadRequest($"Error updating goal: {ex.Message}");
            }
        }

        // DELETE: api/Goals/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGoal(int id)
        {
            Console.WriteLine($"=== GOAL DELETION ===");
            Console.WriteLine($"Attempting to delete goal with ID: {id}");

            var goal = await _context.Goals.FindAsync(id);
            if (goal == null)
            {
                Console.WriteLine($"Goal with ID {id} not found in database");
                return NotFound();
            }

            Console.WriteLine($"Found goal: {goal.Name} (ID: {goal.Id})");
            Console.WriteLine($"Removing goal from database...");

            _context.Goals.Remove(goal);
            await _context.SaveChangesAsync();

            Console.WriteLine($"Goal {id} successfully deleted from database");
            return NoContent();
        }

        // PATCH: api/Goals/5/progress
        [HttpPatch("{id}/progress")]
        public async Task<IActionResult> UpdateGoalProgress(int id, [FromBody] decimal currentAmount)
        {
            var goal = await _context.Goals.FindAsync(id);
            if (goal == null)
            {
                return NotFound();
            }

            if (currentAmount < 0)
            {
                return BadRequest("Current amount cannot be negative");
            }

            goal.CurrentAmount = currentAmount;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DEBUG: Test endpoint to check what data is being received
        [HttpPost("debug")]
        public async Task<ActionResult> DebugCreateGoal([FromBody] object rawData)
        {
            try
            {
                Console.WriteLine("=== DEBUG GOAL CREATION ===");
                Console.WriteLine($"Raw data received: {rawData}");
                Console.WriteLine($"Data type: {rawData.GetType()}");

                // Try to deserialize to CreateGoalDto
                var jsonString = System.Text.Json.JsonSerializer.Serialize(rawData);
                Console.WriteLine($"JSON string: {jsonString}");

                var createGoalDto = System.Text.Json.JsonSerializer.Deserialize<CreateGoalDto>(jsonString);
                Console.WriteLine($"Deserialized DTO: {System.Text.Json.JsonSerializer.Serialize(createGoalDto)}");

                // Check ModelState
                if (!ModelState.IsValid)
                {
                    Console.WriteLine("ModelState is invalid:");
                    foreach (var error in ModelState)
                    {
                        Console.WriteLine($"  {error.Key}: {string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
                    }
                    return BadRequest(ModelState);
                }

                Console.WriteLine("ModelState is valid!");
                return Ok(new { message = "Debug successful", data = createGoalDto });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Debug error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return BadRequest($"Debug error: {ex.Message}");
            }
        }

        private bool GoalExists(int id)
        {
            return _context.Goals.Any(e => e.Id == id);
        }

        private static GoalDto MapToDto(Goal goal)
        {
            var progress = goal.TargetAmount > 0 ? (decimal)(goal.CurrentAmount / goal.TargetAmount * 100) : 0;

            // Calculate remaining days more accurately
            var today = DateTime.UtcNow.Date;
            var endDate = goal.EndDate.Date;
            var remainingDays = (endDate - today).Days;

            return new GoalDto
            {
                Id = goal.Id,
                Name = goal.Name,
                TargetAmount = goal.TargetAmount,
                CurrentAmount = goal.CurrentAmount,
                StartDate = goal.StartDate,
                EndDate = goal.EndDate,
                Description = goal.Description,
                UserId = goal.UserId,
                Progress = Math.Round(progress, 2),
                RemainingDays = remainingDays > 0 ? remainingDays : 0
            };
        }
    }
}

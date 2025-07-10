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
        private readonly ILogger<GoalsController> _logger;

        public GoalsController(ApplicationDbContext context, ILogger<GoalsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/Goals
        [HttpGet]
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

        // POST: api/Goals
        [HttpPost]
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

        // GET: api/Goals/5
        [HttpGet("{id}")]
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
    }
}

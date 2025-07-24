using Microsoft.AspNetCore.Mvc;
using SpendSmart_Backend.DTOs;
using SpendSmart_Backend.Services;

namespace SpendSmart_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GoalController : ControllerBase
    {
        private readonly IGoalService _goalService;

        public GoalController(IGoalService goalService)
        {
            _goalService = goalService;
        }

        /// <summary>
        /// Creates a new goal for the user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="dto">Goal creation data</param>
        /// <returns>Created goal summary</returns>
        [HttpPost("CreateGoal/{userId}")]
        public async Task<ActionResult<GoalSummaryDto>> CreateGoal(int userId, [FromBody] CreateGoalDto dto)
        {
            try
            {
                var result = await _goalService.CreateGoalAsync(userId, dto);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the goal" });
            }
        }

        /// <summary>
        /// Gets detailed summary of a specific goal
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="id">Goal ID</param>
        /// <returns>Goal summary with progress details</returns>
        [HttpGet("GetGoalSummary/{userId}/{id}")]
        public async Task<ActionResult<GoalSummaryDto>> GetGoalSummary(int userId, int id)
        {
            try
            {
                var result = await _goalService.GetGoalSummaryByIdAsync(userId, id);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the goal summary" });
            }
        }

        /// <summary>
        /// Gets all goals for a user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>List of user's goals</returns>
        [HttpGet("GetUserGoals/{userId}")]
        public async Task<ActionResult<List<GoalListDto>>> GetUserGoals(int userId)
        {
            try
            {
                var result = await _goalService.GetUserGoalsAsync(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving goals" });
            }
        }

        /// <summary>
        /// Gets only active goals (not achieved, not overdue)
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>List of active goals</returns>
        [HttpGet("GetActiveGoals/{userId}")]
        public async Task<ActionResult<List<GoalListDto>>> GetActiveGoals(int userId)
        {
            try
            {
                var result = await _goalService.GetActiveGoalsAsync(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving active goals" });
            }
        }

        /// <summary>
        /// Gets achieved goals for a user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>List of achieved goals</returns>
        [HttpGet("GetAchievedGoals/{userId}")]
        public async Task<ActionResult<List<GoalListDto>>> GetAchievedGoals(int userId)
        {
            try
            {
                var result = await _goalService.GetAchievedGoalsAsync(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving achieved goals" });
            }
        }

        /// <summary>
        /// Updates an existing goal
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="id">Goal ID</param>
        /// <param name="dto">Update data</param>
        /// <returns>Updated goal summary</returns>
        [HttpPut("UpdateGoal/{userId}/{id}")]
        public async Task<ActionResult<GoalSummaryDto>> UpdateGoal(int userId, int id, [FromBody] UpdateGoalDto dto)
        {
            try
            {
                var result = await _goalService.UpdateGoalAsync(userId, id, dto);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the goal" });
            }
        }

        /// <summary>
        /// Adds amount to a goal's current amount
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="id">Goal ID</param>
        /// <param name="amount">Amount to add</param>
        /// <returns>Updated goal summary</returns>
        [HttpPost("AddAmount/{userId}/{id}")]
        public async Task<ActionResult<GoalSummaryDto>> AddAmountToGoal(int userId, int id, [FromBody] decimal amount)
        {
            try
            {
                var result = await _goalService.AddAmountToGoalAsync(userId, id, amount);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while adding amount to the goal" });
            }
        }

        /// <summary>
        /// Deletes a goal
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="id">Goal ID</param>
        /// <returns>Success status</returns>
        [HttpDelete("DeleteGoal/{userId}/{id}")]
        public async Task<ActionResult> DeleteGoal(int userId, int id)
        {
            try
            {
                var result = await _goalService.DeleteGoalAsync(userId, id);
                if (!result)
                {
                    return NotFound(new { message = "Goal not found" });
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the goal" });
            }
        }

        /// <summary>
        /// Gets the total savings amount and summary from all active goals
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>Savings summary with total current amount and details</returns>
        [HttpGet("GetTotalSavings/{userId}")]
        public async Task<ActionResult<SavingsSummaryDto>> GetTotalSavings(int userId)
        {
            try
            {
                var result = await _goalService.GetTotalSavingsAsync(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving total savings" });
            }
        }

        /// <summary>
        /// Gets only the total savings amount as a simple decimal value
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>Total current amount from all active goals</returns>
        [HttpGet("GetTotalSavingsAmount/{userId}")]
        public async Task<ActionResult<decimal>> GetTotalSavingsAmount(int userId)
        {
            try
            {
                var result = await _goalService.GetTotalSavingsAmountAsync(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving total savings amount" });
            }
        }
    }
}
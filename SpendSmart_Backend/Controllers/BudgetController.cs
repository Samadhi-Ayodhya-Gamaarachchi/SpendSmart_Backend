using Microsoft.AspNetCore.Mvc;
using SpendSmart_Backend.DTOs;
using SpendSmart_Backend.Services;

namespace SpendSmart_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BudgetController : ControllerBase
    {
        private readonly IBudgetService _budgetService;

        public BudgetController(IBudgetService budgetService)
        {
            _budgetService = budgetService;
        }

        /// <summary>
        /// Creates a new budget for the user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="dto">Budget creation data</param>
        /// <returns>Created budget summary</returns>
        [HttpPost("CreateBudget/{userId}")]
        public async Task<ActionResult<BudgetSummaryDto>> CreateBudget(int userId, [FromBody] CreateBudgetDto dto)
        {
            try
            {
                var result = await _budgetService.CreateBudgetAsync(userId, dto);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the budget" });
            }
        }

        /// <summary>
        /// Gets detailed summary of a specific budget
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="id">Budget ID</param>
        /// <returns>Budget summary with spending details and percentages</returns>
        [HttpGet("GetBudgetSummary/{userId}/{id}")]
        public async Task<ActionResult<BudgetSummaryDto>> GetBudgetSummary(int userId, int id)
        {
            try
            {
                var result = await _budgetService.GetBudgetSummaryByIdAsync(userId, id);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the budget summary" });
            }
        }

        /// <summary>
        /// Gets all budgets for a user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>List of user's budgets</returns>
        [HttpGet("GetUserBudgets/{userId}")]
        public async Task<ActionResult<List<BudgetListDto>>> GetUserBudgets(int userId)
        {
            try
            {
                var result = await _budgetService.GetUserBudgetsAsync(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving budgets" });
            }
        }

        /// <summary>
        /// Gets budget overview for a specific month
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="year">Year</param>
        /// <param name="month">Month (1-12)</param>
        /// <returns>Budget overview with totals and percentages</returns>
        [HttpGet("GetBudgetOverview/{userId}/{year}/{month}")]
        public async Task<ActionResult<BudgetOverviewDto>> GetBudgetOverview(int userId, int year, int month)
        {
            try
            {
                var monthYear = new DateTime(year, month, 1);
                var result = await _budgetService.GetBudgetOverviewAsync(userId, monthYear);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving budget overview" });
            }
        }

        /// <summary>
        /// Gets budgets for a specific month
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="year">Year</param>
        /// <param name="month">Month (1-12)</param>
        /// <returns>List of budgets for the specified month</returns>
        [HttpGet("GetBudgetsByMonth/{userId}/{year}/{month}")]
        public async Task<ActionResult<List<BudgetListDto>>> GetBudgetsByMonth(int userId, int year, int month)
        {
            try
            {
                var monthYear = new DateTime(year, month, 1);
                var result = await _budgetService.GetBudgetsByMonthAsync(userId, monthYear);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving budgets for the month" });
            }
        }

        /// <summary>
        /// Gets budgets that are over the allocated amount
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="year">Optional: Year filter</param>
        /// <param name="month">Optional: Month filter (1-12)</param>
        /// <returns>List of over-budget items</returns>
        [HttpGet("GetOverBudgetItems/{userId}")]
        public async Task<ActionResult<List<BudgetListDto>>> GetOverBudgetItems(int userId, [FromQuery] int? year = null, [FromQuery] int? month = null)
        {
            try
            {
                DateTime? monthYear = null;
                if (year.HasValue && month.HasValue)
                {
                    monthYear = new DateTime(year.Value, month.Value, 1);
                }

                var result = await _budgetService.GetOverBudgetItemsAsync(userId, monthYear);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving over-budget items" });
            }
        }

        /// <summary>
        /// Updates an existing budget
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="id">Budget ID</param>
        /// <param name="dto">Update data</param>
        /// <returns>Updated budget summary</returns>
        [HttpPut("UpdateBudget/{userId}/{id}")]
        public async Task<ActionResult<BudgetSummaryDto>> UpdateBudget(int userId, int id, [FromBody] UpdateBudgetDto dto)
        {
            try
            {
                var result = await _budgetService.UpdateBudgetAsync(userId, id, dto);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the budget" });
            }
        }

        /// <summary>
        /// Adds expense amount to a budget's spend amount
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="id">Budget ID</param>
        /// <param name="dto">Expense data</param>
        /// <returns>Updated budget summary</returns>
        [HttpPost("AddExpense/{userId}/{id}")]
        public async Task<ActionResult<BudgetSummaryDto>> AddExpenseToBudget(int userId, int id, [FromBody] AddExpenseToBudgetDto dto)
        {
            try
            {
                var result = await _budgetService.AddExpenseToBudgetAsync(userId, id, dto);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while adding expense to budget" });
            }
        }

        /// <summary>
        /// Updates budget spend amount based on actual transactions
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="categoryId">Category ID</param>
        /// <param name="year">Year</param>
        /// <param name="month">Month (1-12)</param>
        /// <returns>Success status</returns>
        [HttpPost("UpdateSpendFromTransactions/{userId}/{categoryId}/{year}/{month}")]
        public async Task<ActionResult> UpdateBudgetSpendFromTransactions(int userId, int categoryId, int year, int month)
        {
            try
            {
                var monthYear = new DateTime(year, month, 1);
                await _budgetService.UpdateBudgetSpendFromTransactionsAsync(userId, categoryId, monthYear);
                return Ok(new { message = "Budget spend amount updated from transactions" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating budget from transactions" });
            }
        }

        /// <summary>
        /// Deletes a budget
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="id">Budget ID</param>
        /// <returns>Success status</returns>
        [HttpDelete("DeleteBudget/{userId}/{id}")]
        public async Task<ActionResult> DeleteBudget(int userId, int id)
        {
            try
            {
                var result = await _budgetService.DeleteBudgetAsync(userId, id);
                if (!result)
                {
                    return NotFound(new { message = "Budget not found" });
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the budget" });
            }
        }
    }
}
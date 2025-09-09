using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpendSmart_Backend.DTOs;
using SpendSmart_Backend.Models;
using SpendSmart_Backend.Services;
using System.Security.Claims;

namespace SpendSmart_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RecurringTransactionController : ControllerBase
    {
        private readonly IRecurringTransactionService _recurringTransactionService;

        public RecurringTransactionController(IRecurringTransactionService recurringTransactionService)
        {
            _recurringTransactionService = recurringTransactionService;
        }

        //private int GetCurrentUserId()
        //{
        //    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        //    if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        //    {
        //        throw new UnauthorizedAccessException("User ID not found in token");
        //    }
        //    return userId;
        //}

        [HttpPost("CreateRecurringTransaction/{userId}")]
        public async Task<ActionResult<RecurringTransactionDto>> CreateRecurringTransaction(int userId, [FromBody] CreateRecurringTransactionDto dto)
        {
            try
            {
                var result = await _recurringTransactionService.CreateRecurringTransactionAsync(userId, dto);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the recurring transaction" });
            }
        }

        [HttpGet("GetRecurringTransactionById/{userId}/{id}")]
        public async Task<ActionResult<RecurringTransactionDto>> GetRecurringTransactionById(int userId, int id)
        {
            try
            {
                //var userId = GetCurrentUserId();
                var result = await _recurringTransactionService.GetRecurringTransactionByIdAsync(userId, id);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the recurring transaction" });
            }
        }

        [HttpGet("GetRecurringTransactions/{userId}")]
        public async Task<ActionResult<RecurringTransactionDto>> GetRecurringTransaction(int userId)
        {
            try
            {
                //var userId = GetCurrentUserId();
                var result = await _recurringTransactionService.GetRecurringTransactionAsync(userId);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the recurring transaction" });
            }
        }

        //[HttpGet]
        //public async Task<ActionResult<List<RecurringTransactionListDto>>> GetUserRecurringTransactions()
        //{
        //    try
        //    {
        //        var userId = GetCurrentUserId();
        //        var result = await _recurringTransactionService.GetUserRecurringTransactionsAsync(userId);
        //        return Ok(result);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { message = "An error occurred while retrieving recurring transactions" });
        //    }
        //}

        [HttpGet("active/{userId}")]
        public async Task<ActionResult<List<RecurringTransactionListDto>>> GetActiveRecurringTransactions(int userId)
        {
            try
            {
                //var userId = GetCurrentUserId();
                var result = await _recurringTransactionService.GetActiveRecurringTransactionsAsync(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving active recurring transactions" });
            }
        }

        //[HttpPut("{id}")]
        //public async Task<ActionResult<RecurringTransactionDto>> UpdateRecurringTransaction(int id, [FromBody] UpdateRecurringTransactionDto dto)
        //{
        //    try
        //    {
        //        //var userId = GetCurrentUserId();
        //        var result = await _recurringTransactionService.UpdateRecurringTransactionAsync(id, dto);
        //        return Ok(result);
        //    }
        //    catch (ArgumentException ex)
        //    {
        //        return BadRequest(new { message = ex.Message });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { message = "An error occurred while updating the recurring transaction" });
        //    }
        //}

        [HttpDelete("DeleteRecurringTransaction/{userId}/{id}")]
        public async Task<ActionResult> DeleteRecurringTransaction(int userId, int id)
        {
            try
            {
                //var userId = GetCurrentUserId();
                var result = await _recurringTransactionService.DeleteRecurringTransactionAsync(userId, id);

                if (!result)
                {
                    return NotFound(new { message = "Recurring transaction not found" });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the recurring transaction" });
            }
        }

        [HttpPost("process")]
        [AllowAnonymous] // This should be called by a background service or scheduled job
        public async Task<ActionResult> ProcessRecurringTransactions()
        {
            try
            {
                await _recurringTransactionService.ProcessRecurringTransactionsAsync();
                return Ok(new { message = "Recurring transactions processed successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while processing recurring transactions" });
            }
        }

        [HttpGet("transactions/{userId}/{id}")]
        public async Task<ActionResult<List<Transaction>>> GetTransactionsFromRecurringTransaction(int userId, int id)
        {
            try
            {
                //var userId = GetCurrentUserId();
                var result = await _recurringTransactionService.GetTransactionsFromRecurringTransactionAsync(userId, id);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving transactions" });
            }
        }

        //[HttpDelete("transactions/{transactionId}")]
        //public async Task<ActionResult> DeleteTransactionFromRecurring(int transactionId)
        //{
        //    try
        //    {
        //        //var userId = GetCurrentUserId();
        //        var result = await _recurringTransactionService.DeleteTransactionFromRecurringAsync(transactionId);

        //        if (!result)
        //        {
        //            return NotFound(new { message = "Transaction not found or not from recurring transaction" });
        //        }

        //        return NoContent();
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { message = "An error occurred while deleting the transaction" });
        //    }
        //}

        //[HttpGet("summary")]
        //public async Task<ActionResult<RecurringTransactionSummaryDto>> GetRecurringTransactionSummary()
        //{
        //    try
        //    {
        //        //var userId = GetCurrentUserId();
        //        var result = await _recurringTransactionService.GetRecurringTransactionSummaryAsync();
        //        return Ok(result);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { message = "An error occurred while retrieving the summary" });
        //    }
        //}

        [HttpDelete("transactions/{userId}/{id}")]
        public async Task<ActionResult> DeleteAllTransactionsFromRecurring(int userId, int id)
        {
            try
            {
                var result = await _recurringTransactionService.DeleteAllTransactionsFromRecurringAsync(userId, id);

                if (!result)
                {
                    return NotFound(new { message = "Recurring transaction not found" });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting transactions" });
            }
        }
    }
}
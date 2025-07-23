using Microsoft.AspNetCore.Mvc;
using SpendSmart_Backend.DTOs;
using SpendSmart_Backend.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpendSmart_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionController : ControllerBase
    {
        private readonly TransactionService _transactionService;

        public TransactionController(TransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        // GET: api/Transaction/user/{userId}
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<TransactionViewDto>>> GetUserTransactions(int userId)
        {
            try
            {
                var transactions = await _transactionService.GetUserTransactionsAsync(userId);
                return Ok(new { success = true, data = transactions });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving transactions", error = ex.Message });
            }
        }

        // GET: api/Transaction/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<TransactionDetailsDto>> GetTransaction(int id)
        {
            try
            {
                var transaction = await _transactionService.GetTransactionDetailsAsync(id);
                return Ok(new { success = true, data = transaction });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving the transaction", error = ex.Message });
            }
        }

        // POST: api/Transaction?userId={userId}
        [HttpPost]
        public async Task<ActionResult<object>> CreateTransaction([FromQuery] int userId, [FromBody] CreateTransactionDto createTransactionDto)
        {
            try
            {
                var transaction = await _transactionService.CreateTransactionAsync(userId, createTransactionDto);
                return CreatedAtAction(nameof(GetTransaction), new { id = transaction.TransactionId }, new { success = true, data = transaction });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred while creating the transaction", error = ex.Message });
            }
        }

        // PUT: api/Transaction/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTransaction(int id, CreateTransactionDto updateTransactionDto)
        {
            try
            {
                var transaction = await _transactionService.UpdateTransactionAsync(id, updateTransactionDto);
                return Ok(new { success = true, data = transaction });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred while updating the transaction", error = ex.Message });
            }
        }

        // DELETE: api/Transaction/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTransaction(int id)
        {
            try
            {
                await _transactionService.DeleteTransactionAsync(id);
                return Ok(new { success = true, message = "Transaction deleted successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred while deleting the transaction", error = ex.Message });
            }
        }

        // GET: api/Transaction/user/{userId}/range?startDate={startDate}&endDate={endDate}
        [HttpGet("user/{userId}/range")]
        public async Task<ActionResult<IEnumerable<TransactionViewDto>>> GetTransactionsByDateRange(
            int userId,
            [FromQuery] string startDate,
            [FromQuery] string endDate)
        {
            try
            {
                if (string.IsNullOrEmpty(startDate) || string.IsNullOrEmpty(endDate))
                {
                    return BadRequest(new { success = false, message = "Start date and end date are required" });
                }

                var start = DateTime.Parse(startDate);
                var end = DateTime.Parse(endDate);
                var transactions = await _transactionService.GetTransactionsByDateRangeAsync(userId, start, end);
                return Ok(new { success = true, data = transactions });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving transactions", error = ex.Message });
            }
        }
    }
} 
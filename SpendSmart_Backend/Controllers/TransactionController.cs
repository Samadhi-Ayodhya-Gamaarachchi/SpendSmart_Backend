using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SpendSmart_Backend.Data;
using Microsoft.AspNetCore.Authorization;
using SpendSmart_Backend.DTOs;
using SpendSmart_Backend.Models;

using SpendSmart_Backend.Services;

namespace SpendSmart_Backend.Controllers
{   
    [Route("api/[controller]")]
    public class TransactionController : BaseController
    {
        private readonly ITransactionService _transactionService;

        public TransactionController(ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        [HttpPost("CreateTransaction")]
        public async Task<IActionResult> CreateTransaction([FromBody] TransactionDto transactionDto)
        {
            try
            {
                if(transactionDto == null)
                {
                    return BadRequest("Transaction data is null");
                }
                
                // Get user ID from JWT token - no user can create transactions for others
                var userId = GetCurrentUserId();
                var transaction = await _transactionService.CreateTransactionAsync(userId, transactionDto);
                return Ok(transaction);

            }
            catch(UnauthorizedAccessException)
            {
                return Unauthorized("Invalid token or user access");
            }
            catch(Exception ex)
            {
                return StatusCode(500, $"Error creating transaction {ex.Message}");
            }
        }

        [HttpGet("GetTransaction")]
        public async Task<IActionResult> GetTransaction(
            [FromQuery] string? type, 
            [FromQuery] string? category, 
            [FromQuery] DateTime? date,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] bool? amountByDescending,
            [FromQuery] bool? dateByDescending,
            [FromQuery] string? sorting)
        {
            try
            {
                // Get user ID from JWT token - users can only see their own transactions
                var userId = GetCurrentUserId();
                var transactions = await _transactionService.GetTransactionAsync(userId, type, category, date, startDate, endDate, sorting);
                return Ok(transactions);
            }
            catch(UnauthorizedAccessException)
            {
                return Unauthorized("Invalid token or user access");
            }
            catch(Exception ex)
            {
                return StatusCode(500, $"Error fetching transactions {ex.Message}");
            }
        }

        [HttpDelete("DeleteTransaction/{transactionId}")]
        public async Task<IActionResult> DeleteTransaction(int transactionId)
        {
            try
            {
                // Get user ID from JWT token - users can only delete their own transactions
                var userId = GetCurrentUserId();
                var result = await _transactionService.DeleteTransactionAsync(userId, transactionId);
                if (!result)
                    return NotFound("Transaction not found or you don't have permission to delete it");
                return Ok("Transaction deleted successfully");
            }
            catch(UnauthorizedAccessException)
            {
                return Unauthorized("Invalid token or user access");
            }
            catch(Exception ex)
            {
                return StatusCode(500, $"Error deleting transaction {ex.Message}");
            }
        }
    }
}


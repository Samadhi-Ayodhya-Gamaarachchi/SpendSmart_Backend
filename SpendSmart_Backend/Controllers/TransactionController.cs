using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SpendSmart_Backend.Data;

using SpendSmart_Backend.DTOs;
using SpendSmart_Backend.Models;

using SpendSmart_Backend.Services;

namespace SpendSmart_Backend.Controllers
{   
    [ApiController]
    [Route("api/[controller]")]


    public class TransactionController : Controller
    {
        private readonly ITransactionService _transactionService;

        public TransactionController(ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        [HttpPost("CreateTransaction/{userId}")]
        public async Task<IActionResult> CreateTransaction(int userId, [FromBody] TransactionDto transactionDto)
        {
            try
            {
                if(transactionDto == null)
                {
                    return BadRequest("Transaction data is null");
                }
                var transaction = await _transactionService.CreateTransactionAsync(userId, transactionDto);
                return Ok(transaction);

            }
            catch(Exception ex)
            {
                return StatusCode(500, $"Error creating transaction {ex.Message}");
            }
        }

        [HttpGet("GetTransaction/{userId}")]
        public async Task<IActionResult> GetTransaction(
            int userId,
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
                var transactions = await _transactionService.GetTransactionAsync(userId, type, category, date, startDate, endDate, sorting);
                return Ok(transactions);
            }
            catch(Exception ex)
            {
                return StatusCode(500, $"Error fetching transactions {ex.Message}");
            }
        }

        [HttpDelete("DeleteTransaction/{userId}/{transactionId}")]
        public async Task<IActionResult> DeleteTransaction(int userId, int transactionId)
        {
            try
            {
                var result = await _transactionService.DeleteTransactionAsync(userId, transactionId);
                if (!result)
                    return NotFound("Transaction not found");
                return Ok("Transaction deleted successfully");
            }
            catch(Exception ex)
            {
                return StatusCode(500, $"Error deleting transaction {ex.Message}");
            }
        }
    }
}


using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SpendSmart_Backend.Data;
using SpendSmart_Backend.DTOs;
using SpendSmart_Backend.Models;

namespace SpendSmart_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TransactionController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("CreateTransaction")]
        public async Task<IActionResult> CreateTransaction([FromBody] TransactionDto transactionDto)
        {
            if (transactionDto == null)
            {
                return BadRequest("Transaction data is null.");
            }
            var transaction = new Transaction
            {
                Type = transactionDto.Type,
                CategoryId = transactionDto.CategoryId,
                Amount = transactionDto.Amount,
                Date = transactionDto.Date,
                Description = transactionDto.Description,
                UserId = transactionDto.UserId
            };
            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();
            return Ok(transaction);
        }

        [HttpGet("GetTransaction")]
        public async Task<IActionResult> GetTransactions()
        {
            var transactions = await _context.Transactions
                .Include(t => t.Category)
                .OrderByDescending(t => t.Date)
                .Select(t => new TransactionViewDto
                {
                    Id = t.Id,
                    Type = t.Type,
                    Category = t.Category.Name,
                    Amount = t.Amount,
                    Date = t.Date.ToString("yyyy-MM-dd"),
                    Description = t.Description,
                })
                .ToListAsync();

            return Ok(transactions);
        }

        [HttpDelete("DeleteTransaction/{id}")]
        public async Task<IActionResult> DeleteTransaction(int id)
        {
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction == null)
            {
                return NotFound("Transaction not found.");
            }
            _context.Transactions.Remove(transaction);
            await _context.SaveChangesAsync();
            return Ok("Transaction deleted successfully.");
        }
    }
}

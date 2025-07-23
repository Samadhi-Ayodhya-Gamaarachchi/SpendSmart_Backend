using Microsoft.EntityFrameworkCore;
using SpendSmart_Backend.Data;
using SpendSmart_Backend.DTOs;
using SpendSmart_Backend.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SpendSmart_Backend.Services
{
    public class TransactionService: ITransactionService
    {
        private readonly ApplicationDbContext _context;

        public TransactionService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Transaction> CreateTransactionAsync(TransactionDto dto)
        {
            var transaction = new Transaction
            {
                Type = dto.Type,
                CategoryId = dto.CategoryId,
                Amount = dto.Amount,
                Date = dto.Date,
                Description = dto.Description,
                UserId = dto.UserId,
            };
            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();
            return transaction;
        }

        public async Task<List<TransactionViewDto>> GetTransactionAsync(
            string? type = null,
            string? category = null,
            DateTime? date = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            string? sorting = null)
        {
            var transactions = _context.Transactions
                .Include(t => t.Category)
                .OrderByDescending(t => t.Date)
                .AsQueryable();

            // Filtering
            if (!string.IsNullOrWhiteSpace(type))
            {
                transactions = transactions.Where(t => t.Type.Contains(type));
            }
            if (!string.IsNullOrWhiteSpace(category))
            {
                transactions = transactions.Where(t => t.Category.Name.Contains(category));
            }

            DateTime? effectiveStartDate = startDate;
            DateTime? effectiveEndDate = endDate;

            if (effectiveStartDate.HasValue && !effectiveEndDate.HasValue)
            {
                effectiveEndDate = DateTime.Today;
            }

            if (effectiveStartDate.HasValue && effectiveEndDate.HasValue)
            {
                var start = effectiveStartDate.Value.Date;
                var end = effectiveEndDate.Value.Date;
                transactions = transactions.Where(t => t.Date.Date >= start && t.Date.Date <= end);
            }
            else if (date.HasValue)
            {
                var targetDate = date.Value.Date;
                transactions = transactions.Where(t => t.Date.Date == targetDate);
            }

            // Sorting
            if (!string.IsNullOrWhiteSpace(sorting))
            {
                transactions = sorting switch
                {
                    "newest" => transactions.OrderByDescending(t => t.Date),
                    "oldest" => transactions.OrderBy(t => t.Date),
                    "higher-amount" => transactions.OrderByDescending(t => t.Amount),
                    "lower-amount" => transactions.OrderBy(t => t.Amount),
                    _ => transactions // Default case - no sorting
                };
            }

            var result = await transactions
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

            return result;
        }

        public async Task<bool> DeleteTransactionAsync(int transactionId)
        {
            var transaction = await _context.Transactions.FindAsync(transactionId);
            if (transaction == null)
                return false;
            _context.Transactions.Remove(transaction);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}

// Services/TransactionService.cs
using Microsoft.EntityFrameworkCore;
using SpendSmart_Backend.Data;
using SpendSmart_Backend.Models;
using SpendSmart_Backend.Services;

namespace SpendSmart_Backend.Services
{
    public interface ITransactionService
    {
        Task<Transaction> CreateTransactionAsync(Transaction transaction);
        Task<Transaction> UpdateTransactionAsync(Transaction transaction);
        Task<bool> DeleteTransactionAsync(int transactionId);
        Task<List<Transaction>> GetUserTransactionsAsync(int userId);
        Task<Transaction> GetTransactionByIdAsync(int transactionId);
    }

    public class TransactionService : ITransactionService
    {
        private readonly ApplicationDbContext _context;
        private readonly IBudgetService _budgetService;

        public TransactionService(ApplicationDbContext context, IBudgetService budgetService)
        {
            _context = context;
            _budgetService = budgetService;
        }

        public async Task<Transaction> CreateTransactionAsync(Transaction transaction)
        {
            using var dbTransaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Add transaction
                _context.Transactions.Add(transaction);
                await _context.SaveChangesAsync();

                // If it's an expense transaction, update affected budgets
                if (transaction.TransactionType.ToLower() == "expense")
                {
                    await _budgetService.ProcessBudgetImpactAsync(
                        transaction.UserId,
                        transaction.CategoryId,
                        transaction.Amount,
                        transaction.TransactionDate,
                        transaction.TransactionId);
                }

                await dbTransaction.CommitAsync();
                return transaction;
            }
            catch
            {
                await dbTransaction.RollbackAsync();
                throw;
            }
        }

        public async Task<Transaction> UpdateTransactionAsync(Transaction updatedTransaction)
        {
            using var dbTransaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var existingTransaction = await _context.Transactions
                    .FirstOrDefaultAsync(t => t.TransactionId == updatedTransaction.TransactionId);

                if (existingTransaction == null)
                    throw new ArgumentException("Transaction not found");

                // Store old values for budget adjustment
                var oldAmount = existingTransaction.Amount;
                var oldCategoryId = existingTransaction.CategoryId;
                var oldTransactionDate = existingTransaction.TransactionDate;
                var oldTransactionType = existingTransaction.TransactionType;

                // Update transaction
                existingTransaction.CategoryId = updatedTransaction.CategoryId;
                existingTransaction.TransactionType = updatedTransaction.TransactionType;
                existingTransaction.Amount = updatedTransaction.Amount;
                existingTransaction.Description = updatedTransaction.Description;
                existingTransaction.TransactionDate = updatedTransaction.TransactionDate;
                existingTransaction.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Update budget impacts
                await UpdateBudgetImpactsForModifiedTransaction(
                    existingTransaction.TransactionId,
                    existingTransaction.UserId,
                    oldCategoryId,
                    oldAmount,
                    oldTransactionDate,
                    oldTransactionType,
                    updatedTransaction.CategoryId,
                    updatedTransaction.Amount,
                    updatedTransaction.TransactionDate,
                    updatedTransaction.TransactionType);

                await dbTransaction.CommitAsync();
                return existingTransaction;
            }
            catch
            {
                await dbTransaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> DeleteTransactionAsync(int transactionId)
        {
            using var dbTransaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var transaction = await _context.Transactions
                    .FirstOrDefaultAsync(t => t.TransactionId == transactionId);

                if (transaction == null)
                    return false;

                // Remove budget impacts if it was an expense
                if (transaction.TransactionType.ToLower() == "expense")
                {
                    await _budgetService.ReverseBudgetImpactAsync(transactionId);
                }

                _context.Transactions.Remove(transaction);
                await _context.SaveChangesAsync();

                await dbTransaction.CommitAsync();
                return true;
            }
            catch
            {
                await dbTransaction.RollbackAsync();
                throw;
            }
        }

        public async Task<List<Transaction>> GetUserTransactionsAsync(int userId)
        {
            return await _context.Transactions
                .Include(t => t.Category)
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();
        }

        public async Task<Transaction> GetTransactionByIdAsync(int transactionId)
        {
            return await _context.Transactions
                .Include(t => t.Category)
                .FirstOrDefaultAsync(t => t.TransactionId == transactionId);
        }

        private async Task UpdateBudgetImpactsForModifiedTransaction(
            int transactionId,
            int userId,
            int oldCategoryId,
            decimal oldAmount,
            DateTime oldTransactionDate,
            string oldTransactionType,
            int newCategoryId,
            decimal newAmount,
            DateTime newTransactionDate,
            string newTransactionType)
        {
            // Remove old budget impacts if it was an expense
            if (oldTransactionType.ToLower() == "expense")
            {
                await _budgetService.ReverseBudgetImpactAsync(transactionId);
            }

            // Add new budget impacts if it's now an expense
            if (newTransactionType.ToLower() == "expense")
            {
                await _budgetService.ProcessBudgetImpactAsync(
                    userId,
                    newCategoryId,
                    newAmount,
                    newTransactionDate,
                    transactionId);
            }
        }
    }
}
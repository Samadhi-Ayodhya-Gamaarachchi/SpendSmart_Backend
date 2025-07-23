using Microsoft.EntityFrameworkCore;
using SpendSmart_Backend.Data;
using SpendSmart_Backend.DTOs;
using SpendSmart_Backend.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpendSmart_Backend.Services
{
    public class TransactionService
    {
        private readonly ApplicationDbContext _context;
        private readonly BudgetService _budgetService;

        public TransactionService(ApplicationDbContext context, BudgetService budgetService)
        {
            _context = context;
            _budgetService = budgetService;
        }

        public async Task<List<TransactionViewDto>> GetUserTransactionsAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {userId} not found");
            }

            var transactions = await _context.Transactions
                .Include(t => t.Category)
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.TransactionDate)
                .Select(t => new TransactionViewDto
                {
                    Id = t.TransactionId,
                    Type = t.TransactionType,
                    Category = t.Category.CategoryName,
                    Amount = t.Amount,
                    Date = t.TransactionDate.ToString("yyyy-MM-dd"),
                    Description = t.Description,
                    UserId = t.UserId
                })
                .ToListAsync();

            return transactions;
        }

        public async Task<TransactionDetailsDto> GetTransactionDetailsAsync(int transactionId)
        {
            var transaction = await _context.Transactions
                .Include(t => t.Category)
                .Include(t => t.TransactionBudgetImpacts)
                .ThenInclude(tbi => tbi.Budget)
                .FirstOrDefaultAsync(t => t.TransactionId == transactionId);

            if (transaction == null)
            {
                throw new KeyNotFoundException($"Transaction with ID {transactionId} not found");
            }

            // Get budget impacts
            var budgetImpacts = transaction.TransactionBudgetImpacts.Select(tbi => new BudgetImpactDto
            {
                BudgetId = tbi.BudgetId,
                BudgetName = tbi.Budget.BudgetName,
                CategoryId = tbi.CategoryId,
                CategoryName = tbi.Category.CategoryName,
                ImpactAmount = tbi.ImpactAmount,
                ImpactType = "Deduction" // Assuming all impacts are deductions for now
            }).ToList();

            // Convert tags from comma-separated string to array
            string[] tags = transaction.Tags?.Split(',', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();

            return new TransactionDetailsDto
            {
                TransactionId = transaction.TransactionId,
                TransactionType = transaction.TransactionType,
                CategoryId = transaction.CategoryId,
                CategoryName = transaction.Category.CategoryName,
                Amount = transaction.Amount,
                TransactionDate = transaction.TransactionDate.ToString("yyyy-MM-dd"),
                Description = transaction.Description,
                MerchantName = transaction.MerchantName,
                Location = transaction.Location,
                Tags = tags,
                ReceiptUrl = transaction.ReceiptUrl,
                IsRecurring = transaction.IsRecurring,
                RecurringFrequency = transaction.RecurringFrequency,
                RecurringEndDate = transaction.RecurringEndDate?.ToString("yyyy-MM-dd"),
                BudgetImpacts = budgetImpacts
            };
        }

        public async Task<TransactionDetailsDto> CreateTransactionAsync(int userId, CreateTransactionDto createTransactionDto)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {userId} not found");
            }

            var category = await _context.Categories.FindAsync(createTransactionDto.CategoryId);
            if (category == null)
            {
                throw new KeyNotFoundException($"Category with ID {createTransactionDto.CategoryId} not found");
            }

            // Ensure transaction type is either "Income" or "Expense" exactly
            string transactionType = createTransactionDto.TransactionType;
            if (transactionType != "Income" && transactionType != "Expense")
            {
                // Try to normalize the transaction type
                if (transactionType.Equals("income", StringComparison.OrdinalIgnoreCase))
                {
                    transactionType = "Income";
                }
                else if (transactionType.Equals("expense", StringComparison.OrdinalIgnoreCase))
                {
                    transactionType = "Expense";
                }
                else
                {
                    throw new ArgumentException("Transaction type must be either 'Income' or 'Expense'");
                }
            }

            var transaction = new Transaction
            {
                UserId = userId,
                CategoryId = createTransactionDto.CategoryId,
                TransactionType = transactionType, // Use the normalized transaction type
                Amount = createTransactionDto.Amount,
                TransactionDate = DateTime.Parse(createTransactionDto.TransactionDate),
                Description = createTransactionDto.Description,
                MerchantName = createTransactionDto.MerchantName,
                Location = createTransactionDto.Location,
                Tags = createTransactionDto.Tags != null ? string.Join(",", createTransactionDto.Tags) : null,
                ReceiptUrl = createTransactionDto.ReceiptUrl,
                IsRecurring = createTransactionDto.IsRecurring,
                RecurringFrequency = createTransactionDto.RecurringFrequency,
                RecurringEndDate = createTransactionDto.RecurringEndDate != null ? DateTime.Parse(createTransactionDto.RecurringEndDate) : null,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            // Process budget impacts for expense transactions
            var budgetImpacts = new List<BudgetImpactDto>();
            if (transaction.TransactionType == "Expense")
            {
                // Find active budgets that include this category
                var activeBudgets = await _context.Budgets
                    .Include(b => b.BudgetCategories)
                    .Where(b =>
                        b.UserId == userId &&
                        b.Status == "Active" &&
                        b.BudgetCategories.Any(bc => bc.CategoryId == transaction.CategoryId) &&
                        b.StartDate <= transaction.TransactionDate &&
                        (b.EndDate == null || b.EndDate >= transaction.TransactionDate))
                    .ToListAsync();

                foreach (var budget in activeBudgets)
                {
                    // Record impact on this budget
                    await _budgetService.RecordTransactionImpactAsync(
                        transaction.TransactionId,
                        budget.BudgetId,
                        transaction.CategoryId,
                        transaction.Amount
                    );

                    // Add to response
                    budgetImpacts.Add(new BudgetImpactDto
                    {
                        BudgetId = budget.BudgetId,
                        BudgetName = budget.BudgetName,
                        CategoryId = transaction.CategoryId,
                        CategoryName = category.CategoryName,
                        ImpactAmount = transaction.Amount,
                        ImpactType = "Deduction"
                    });
                }

                // Update budget amounts
                foreach (var budget in activeBudgets)
                {
                    await _budgetService.UpdateBudgetAmountsAsync(budget.BudgetId);
                }
            }

            // Create recurring transactions if needed
            if (transaction.IsRecurring && !string.IsNullOrEmpty(transaction.RecurringFrequency) && transaction.RecurringEndDate.HasValue)
            {
                await CreateRecurringTransactionsAsync(transaction);
            }

            // Return transaction details with budget impacts
            return new TransactionDetailsDto
            {
                TransactionId = transaction.TransactionId,
                TransactionType = transaction.TransactionType,
                CategoryId = transaction.CategoryId,
                CategoryName = category.CategoryName,
                Amount = transaction.Amount,
                TransactionDate = transaction.TransactionDate.ToString("yyyy-MM-dd"),
                Description = transaction.Description,
                MerchantName = transaction.MerchantName,
                Location = transaction.Location,
                Tags = createTransactionDto.Tags ?? Array.Empty<string>(),
                ReceiptUrl = transaction.ReceiptUrl,
                IsRecurring = transaction.IsRecurring,
                RecurringFrequency = transaction.RecurringFrequency,
                RecurringEndDate = transaction.RecurringEndDate?.ToString("yyyy-MM-dd"),
                BudgetImpacts = budgetImpacts
            };
        }

        public async Task<TransactionDetailsDto> UpdateTransactionAsync(int transactionId, CreateTransactionDto updateTransactionDto)
        {
            var transaction = await _context.Transactions.FindAsync(transactionId);
            if (transaction == null)
            {
                throw new KeyNotFoundException($"Transaction with ID {transactionId} not found");
            }

            var category = await _context.Categories.FindAsync(updateTransactionDto.CategoryId);
            if (category == null)
            {
                throw new KeyNotFoundException($"Category with ID {updateTransactionDto.CategoryId} not found");
            }

            // Ensure transaction type is either "Income" or "Expense" exactly
            string transactionType = updateTransactionDto.TransactionType;
            if (transactionType != "Income" && transactionType != "Expense")
            {
                // Try to normalize the transaction type
                if (transactionType.Equals("income", StringComparison.OrdinalIgnoreCase))
                {
                    transactionType = "Income";
                }
                else if (transactionType.Equals("expense", StringComparison.OrdinalIgnoreCase))
                {
                    transactionType = "Expense";
                }
                else
                {
                    throw new ArgumentException("Transaction type must be either 'Income' or 'Expense'");
                }
            }

            // Update transaction properties
            transaction.CategoryId = updateTransactionDto.CategoryId;
            transaction.TransactionType = transactionType; // Use the normalized transaction type
            transaction.Amount = updateTransactionDto.Amount;
            transaction.TransactionDate = DateTime.Parse(updateTransactionDto.TransactionDate);
            transaction.Description = updateTransactionDto.Description;
            transaction.MerchantName = updateTransactionDto.MerchantName;
            transaction.Location = updateTransactionDto.Location;
            transaction.Tags = updateTransactionDto.Tags != null ? string.Join(",", updateTransactionDto.Tags) : null;
            transaction.ReceiptUrl = updateTransactionDto.ReceiptUrl;
            transaction.IsRecurring = updateTransactionDto.IsRecurring;
            transaction.RecurringFrequency = updateTransactionDto.RecurringFrequency;
            transaction.RecurringEndDate = updateTransactionDto.RecurringEndDate != null ? DateTime.Parse(updateTransactionDto.RecurringEndDate) : null;
            transaction.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Re-calculate budget impacts
            var budgetImpacts = new List<BudgetImpactDto>();
            if (transaction.TransactionType == "Expense")
            {
                // First, remove existing impacts
                var existingImpacts = await _context.TransactionBudgetImpacts
                    .Where(tbi => tbi.TransactionId == transaction.TransactionId)
                    .ToListAsync();

                _context.TransactionBudgetImpacts.RemoveRange(existingImpacts);
                await _context.SaveChangesAsync();

                // Find active budgets that include this category
                var activeBudgets = await _context.Budgets
                    .Include(b => b.BudgetCategories)
                    .Where(b =>
                        b.UserId == transaction.UserId &&
                        b.Status == "Active" &&
                        b.BudgetCategories.Any(bc => bc.CategoryId == transaction.CategoryId) &&
                        b.StartDate <= transaction.TransactionDate &&
                        (b.EndDate == null || b.EndDate >= transaction.TransactionDate))
                    .ToListAsync();

                // Create new impacts
                foreach (var budget in activeBudgets)
                {
                    await _budgetService.RecordTransactionImpactAsync(
                        transaction.TransactionId,
                        budget.BudgetId,
                        transaction.CategoryId,
                        transaction.Amount
                    );

                    // Add to response
                    budgetImpacts.Add(new BudgetImpactDto
                    {
                        BudgetId = budget.BudgetId,
                        BudgetName = budget.BudgetName,
                        CategoryId = transaction.CategoryId,
                        CategoryName = category.CategoryName,
                        ImpactAmount = transaction.Amount,
                        ImpactType = "Deduction"
                    });
                }

                // Update budget amounts
                foreach (var budget in activeBudgets)
                {
                    await _budgetService.UpdateBudgetAmountsAsync(budget.BudgetId);
                }
            }

            // Return updated transaction details
            return await GetTransactionDetailsAsync(transactionId);
        }

        public async Task DeleteTransactionAsync(int transactionId)
        {
            var transaction = await _context.Transactions
                .Include(t => t.TransactionBudgetImpacts)
                .FirstOrDefaultAsync(t => t.TransactionId == transactionId);

            if (transaction == null)
            {
                throw new KeyNotFoundException($"Transaction with ID {transactionId} not found");
            }

            // Get affected budget IDs before removing the transaction
            var affectedBudgetIds = transaction.TransactionBudgetImpacts
                .Select(tbi => tbi.BudgetId)
                .Distinct()
                .ToList();

            _context.Transactions.Remove(transaction);
            await _context.SaveChangesAsync();

            // Update affected budgets
            foreach (var budgetId in affectedBudgetIds)
            {
                await _budgetService.UpdateBudgetAmountsAsync(budgetId);
            }
        }

        public async Task<List<TransactionViewDto>> GetTransactionsByDateRangeAsync(int userId, DateTime startDate, DateTime endDate)
        {
            var transactions = await _context.Transactions
                .Include(t => t.Category)
                .Where(t =>
                    t.UserId == userId &&
                    t.TransactionDate >= startDate &&
                    t.TransactionDate <= endDate)
                .OrderByDescending(t => t.TransactionDate)
                .Select(t => new TransactionViewDto
                {
                    Id = t.TransactionId,
                    Type = t.TransactionType,
                    Category = t.Category.CategoryName,
                    Amount = t.Amount,
                    Date = t.TransactionDate.ToString("yyyy-MM-dd"),
                    Description = t.Description,
                    UserId = t.UserId
                })
                .ToListAsync();

            return transactions;
        }

        private async Task CreateRecurringTransactionsAsync(Transaction baseTransaction)
        {
            if (!baseTransaction.IsRecurring || string.IsNullOrEmpty(baseTransaction.RecurringFrequency) || !baseTransaction.RecurringEndDate.HasValue)
            {
                return;
            }

            DateTime startDate = baseTransaction.TransactionDate;
            DateTime endDate = baseTransaction.RecurringEndDate.Value;
            DateTime currentDate = startDate;

            // Calculate next date based on frequency
            while (true)
            {
                // Calculate next date based on frequency
                currentDate = baseTransaction.RecurringFrequency switch
                {
                    "Daily" => currentDate.AddDays(1),
                    "Weekly" => currentDate.AddDays(7),
                    "Monthly" => currentDate.AddMonths(1),
                    "Annually" => currentDate.AddYears(1),
                    _ => currentDate.AddMonths(1) // Default to monthly
                };

                // Stop if we've gone past the end date
                if (currentDate > endDate)
                {
                    break;
                }

                // Create a new transaction for this date
                var recurringTransaction = new Transaction
                {
                    UserId = baseTransaction.UserId,
                    CategoryId = baseTransaction.CategoryId,
                    TransactionType = baseTransaction.TransactionType,
                    Amount = baseTransaction.Amount,
                    TransactionDate = currentDate,
                    Description = baseTransaction.Description,
                    MerchantName = baseTransaction.MerchantName,
                    Location = baseTransaction.Location,
                    Tags = baseTransaction.Tags,
                    ReceiptUrl = baseTransaction.ReceiptUrl,
                    IsRecurring = false, // Individual instances are not themselves recurring
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Transactions.Add(recurringTransaction);
            }

            await _context.SaveChangesAsync();

            // Process budget impacts for all created transactions
            if (baseTransaction.TransactionType == "Expense")
            {
                var recurringTransactions = await _context.Transactions
                    .Where(t => 
                        t.UserId == baseTransaction.UserId && 
                        t.CategoryId == baseTransaction.CategoryId && 
                        t.TransactionDate > baseTransaction.TransactionDate && 
                        t.TransactionDate <= endDate)
                    .ToListAsync();

                foreach (var transaction in recurringTransactions)
                {
                    // Find active budgets that include this category for this date
                    var activeBudgets = await _context.Budgets
                        .Include(b => b.BudgetCategories)
                        .Where(b =>
                            b.UserId == transaction.UserId &&
                            b.Status == "Active" &&
                            b.BudgetCategories.Any(bc => bc.CategoryId == transaction.CategoryId) &&
                            b.StartDate <= transaction.TransactionDate &&
                            (b.EndDate == null || b.EndDate >= transaction.TransactionDate))
                        .ToListAsync();

                    foreach (var budget in activeBudgets)
                    {
                        // Record impact on this budget
                        await _budgetService.RecordTransactionImpactAsync(
                            transaction.TransactionId,
                            budget.BudgetId,
                            transaction.CategoryId,
                            transaction.Amount
                        );

                        // Update budget amounts
                        await _budgetService.UpdateBudgetAmountsAsync(budget.BudgetId);
                    }
                }
            }
        }
    }
} 
using Microsoft.EntityFrameworkCore;
using SpendSmart_Backend.Data;
using SpendSmart_Backend.DTOs;
using SpendSmart_Backend.Models;

namespace SpendSmart_Backend.Services
{
    public class RecurringTransactionService : IRecurringTransactionService
    {
        private readonly ApplicationDbContext _context;

        public RecurringTransactionService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<RecurringTransactionDto> CreateRecurringTransactionAsync(CreateRecurringTransactionDto dto)
        {
            // Validate category exists and belongs to user
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == dto.CategoryId);

            if (category == null)
            {
                throw new ArgumentException("Category not found or doesn't belong to user");
            }

            // Validate frequency
            var validFrequencies = new[] { "Daily", "Weekly", "Monthly", "Yearly" };
            if (!validFrequencies.Contains(dto.Frequency))
            {
                throw new ArgumentException("Invalid frequency. Must be Daily, Weekly, Monthly, or Yearly");
            }

            // Validate type
            var validTypes = new[] { "Income", "Expense" };
            if (!validTypes.Contains(dto.Type))
            {
                throw new ArgumentException("Invalid type. Must be Income or Expense");
            }

            // Validate date logic
            if (dto.EndDate.HasValue && dto.EndDate <= dto.StartDate)
            {
                throw new ArgumentException("End date must be after start date");
            }

            // New validation for EndDate/Occurrences
            if ((dto.EndDate == null && dto.Occurrences == null) || (dto.EndDate != null && dto.Occurrences != null))
            {
                throw new ArgumentException("Either EndDate or Occurrences must be provided, but not both.");
            }

            var recurringTransaction = new RecurringTransaction
            {
                Type = dto.Type,
                CategoryId = dto.CategoryId,
                Amount = dto.Amount,
                Description = dto.Description,
                Frequency = dto.Frequency,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Occurrences = dto.Occurrences,
                AutoDeduction = dto.AutoDeduction,
                UserId = dto.UserId
            };

            _context.RecurringTransactions.Add(recurringTransaction);
            await _context.SaveChangesAsync();
            if (dto.AutoDeduction && dto.StartDate.Date == DateTime.Today)
            {
                var transaction = new Transaction
                {
                    Type = dto.Type,
                    CategoryId = dto.CategoryId,
                    Amount = dto.Amount,
                    Date = DateTime.Today,
                    Description = $"{dto.Description} (Recurring Transaction)",
                    UserId = dto.UserId,
                    RecurringTransactionId = recurringTransaction.Id
                };

                _context.Transactions.Add(transaction);
                await _context.SaveChangesAsync();
            }

            return await GetRecurringTransactionByIdAsync(recurringTransaction.Id);
            //return recurringTransaction;
        }

        public async Task<RecurringTransactionDto> GetRecurringTransactionByIdAsync(int id)
        {
            var recurringTransaction = await _context.RecurringTransactions
                .Include(static rt => rt.Category)
                .FirstOrDefaultAsync(rt => rt.Id == id);

            if (recurringTransaction == null)
            {
                throw new ArgumentException("Recurring transaction not found");
            }

            return new RecurringTransactionDto
            {
                Id = recurringTransaction.Id,
                Type = recurringTransaction.Type,
                CategoryId = recurringTransaction.CategoryId,
                CategoryName = recurringTransaction.Category.Name,
                Amount = recurringTransaction.Amount,
                Frequency = recurringTransaction.Frequency,
                StartDate = recurringTransaction.StartDate,
                EndDate = recurringTransaction.EndDate,
                Occurrences = recurringTransaction.Occurrences,
                AutoDeduction = recurringTransaction.AutoDeduction,
                UserId = recurringTransaction.UserId,
                NextExecutionDate = CalculateNextExecutionDate(recurringTransaction),
                IsActive = IsRecurringTransactionActive(recurringTransaction)
            };
        }

        public async Task<List<RecurringTransactionDto>> GetRecurringTransactionAsync()
        {
            var recurringTransactions = await _context.RecurringTransactions
                .Include(rt => rt.Category)
                .ToListAsync();

            return recurringTransactions.Select(rt => new RecurringTransactionDto
            {
                Id = rt.Id,
                Type = rt.Type,
                CategoryId = rt.CategoryId,
                CategoryName = rt.Category?.Name,
                Amount = rt.Amount,
                Description = rt.Description,
                Frequency = rt.Frequency,
                StartDate = rt.StartDate,
                EndDate = rt.EndDate,
                Occurrences = rt.Occurrences,
                AutoDeduction = rt.AutoDeduction,
                UserId = rt.UserId,
                NextExecutionDate = CalculateNextExecutionDate(rt),
                IsActive = IsRecurringTransactionActive(rt)
            }).ToList();
        }


        //public async Task<List<RecurringTransactionListDto>> GetUserRecurringTransactionsAsync(int userId)
        //{
        //    var recurringTransactions = await _context.RecurringTransactions
        //        .Include(rt => rt.Category)
        //        .Where(rt => rt.UserId == userId)
        //        .OrderBy(rt => rt.StartDate)
        //        .ToListAsync();

        //    return recurringTransactions.Select(rt => new RecurringTransactionListDto
        //    {
        //        Id = rt.Id,
        //        Type = rt.Type,
        //        CategoryName = rt.Category.Name,
        //        Amount = rt.Amount,
        //        Frequency = rt.Frequency,
        //        StartDate = rt.StartDate,
        //        EndDate = rt.EndDate,
        //        AutoDeduction = rt.AutoDeduction,
        //        NextExecutionDate = CalculateNextExecutionDate(rt),
        //        IsActive = IsRecurringTransactionActive(rt),
        //        GeneratedTransactionsCount = _context.Transactions.Count(t => t.RecurringTransactionId == rt.Id)
        //    }).ToList();
        //}

        public async Task<List<RecurringTransactionListDto>> GetActiveRecurringTransactionsAsync()
        {
            var recurringTransactions = await _context.RecurringTransactions
                .Include(rt => rt.Category)
                .ToListAsync();

            return recurringTransactions
                .Where(rt => IsRecurringTransactionActive(rt))
                .Select(rt => new RecurringTransactionListDto
                {
                    Id = rt.Id,
                    Type = rt.Type,
                    CategoryName = rt.Category.Name,
                    Amount = rt.Amount,
                    Frequency = rt.Frequency,
                    StartDate = rt.StartDate,
                    EndDate = rt.EndDate,
                    AutoDeduction = rt.AutoDeduction,
                    NextExecutionDate = CalculateNextExecutionDate(rt),
                    IsActive = true,
                    GeneratedTransactionsCount = _context.Transactions.Count(t => t.RecurringTransactionId == rt.Id)
                })
                .OrderBy(rt => rt.NextExecutionDate)
                .ToList();
        }

        //public async Task<RecurringTransactionDto> UpdateRecurringTransactionAsync(int id, UpdateRecurringTransactionDto dto)
        //{
        //    var recurringTransaction = await _context.RecurringTransactions
        //        .FirstOrDefaultAsync(rt => rt.Id == id);

        //    if (recurringTransaction == null)
        //    {
        //        throw new ArgumentException("Recurring transaction not found");
        //    }

        //    // Update fields if provided
        //    if (!string.IsNullOrEmpty(dto.Type))
        //    {
        //        var validTypes = new[] { "Income", "Expense" };
        //        if (!validTypes.Contains(dto.Type))
        //        {
        //            throw new ArgumentException("Invalid type. Must be Income or Expense");
        //        }
        //        recurringTransaction.Type = dto.Type;
        //    }

        //    if (dto.CategoryId.HasValue)
        //    {
        //        var category = await _context.Categories
        //            .FirstOrDefaultAsync(c => c.Id == dto.CategoryId);
        //        if (category == null)
        //        {
        //            throw new ArgumentException("Category not found or doesn't belong to user");
        //        }
        //        recurringTransaction.CategoryId = dto.CategoryId.Value;
        //    }

        //    if (dto.Amount.HasValue)
        //    {
        //        recurringTransaction.Amount = dto.Amount.Value;
        //    }

        //    if (!string.IsNullOrEmpty(dto.Frequency))
        //    {
        //        var validFrequencies = new[] { "Daily", "Weekly", "Monthly", "Yearly" };
        //        if (!validFrequencies.Contains(dto.Frequency))
        //        {
        //            throw new ArgumentException("Invalid frequency. Must be Daily, Weekly, Monthly, or Yearly");
        //        }
        //        recurringTransaction.Frequency = dto.Frequency;
        //    }

        //    if (dto.StartDate.HasValue)
        //    {
        //        recurringTransaction.StartDate = dto.StartDate;
        //    }

        //    if (dto.EndDate.HasValue)
        //    {
        //        if (dto.EndDate <= recurringTransaction.StartDate)
        //        {
        //            throw new ArgumentException("End date must be after start date");
        //        }
        //        recurringTransaction.EndDate = dto.EndDate;
        //    }

        //    if (dto.Occurrences.HasValue)
        //    {
        //        recurringTransaction.Occurrences = dto.Occurrences;
        //    }

        //    if (dto.AutoDeduction.HasValue)
        //    {
        //        recurringTransaction.AutoDeduction = dto.AutoDeduction.Value;
        //    }

        //    await _context.SaveChangesAsync();

        //    return await GetRecurringTransactionByIdAsync(id);
        //}

        public async Task<bool> DeleteRecurringTransactionAsync(int id)
        {
            var recurringTransaction = await _context.RecurringTransactions
                .FirstOrDefaultAsync(rt => rt.Id == id);

            if (recurringTransaction == null)
            {
                return false;
            }

            _context.RecurringTransactions.Remove(recurringTransaction);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task ProcessRecurringTransactionsAsync()
        {
            var today = DateTime.Today;
            var activeRecurringTransactions = await _context.RecurringTransactions
                .Include(rt => rt.Category)
                .Where(rt => rt.StartDate <= today)
                .ToListAsync();

            foreach (var recurringTransaction in activeRecurringTransactions)
            {
                if (!IsRecurringTransactionActive(recurringTransaction))
                    continue;

                var nextExecutionDate = CalculateNextExecutionDate(recurringTransaction);

                if (nextExecutionDate?.Date == today && recurringTransaction.AutoDeduction)
                {
                    // Check if transaction for today already exists
                    var existingTransaction = await _context.Transactions
                        .FirstOrDefaultAsync(t => t.RecurringTransactionId == recurringTransaction.Id &&
                                                 t.Date.Date == today);

                    if (existingTransaction == null)
                    {
                        // Create a new transaction
                        var transaction = new Transaction
                        {
                            Type = recurringTransaction.Type,
                            CategoryId = recurringTransaction.CategoryId,
                            Amount = recurringTransaction.Amount,
                            Date = today,
                            Description = $"Recurring Transaction ({recurringTransaction.Frequency})",
                            UserId = recurringTransaction.UserId,
                            RecurringTransactionId = recurringTransaction.Id
                        };

                        _context.Transactions.Add(transaction);
                    }
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task<List<Transaction>> GetTransactionsFromRecurringTransactionAsync(int recurringTransactionId)
        {
            // Verify the recurring transaction belongs to the user
            var recurringTransaction = await _context.RecurringTransactions
                .FirstOrDefaultAsync(rt => rt.Id == recurringTransactionId);

            if (recurringTransaction == null)
            {
                throw new ArgumentException("Recurring transaction not found");
            }

            return await _context.Transactions
                .Include(t => t.Category)
                .Where(t => t.RecurringTransactionId == recurringTransactionId)
                .OrderByDescending(t => t.Date)
                .ToListAsync();
        }

        public async Task<bool> DeleteTransactionFromRecurringAsync(int transactionId)
        {
            var transaction = await _context.Transactions
                .FirstOrDefaultAsync(t => t.Id == transactionId && t.RecurringTransactionId.HasValue);

            if (transaction == null)
            {
                return false;
            }

            _context.Transactions.Remove(transaction);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<RecurringTransactionSummaryDto> GetRecurringTransactionSummaryAsync()
        {
            var recurringTransactions = await _context.RecurringTransactions
                .Include(rt => rt.Category)
                .ToListAsync();

            var activeTransactions = recurringTransactions.Where(rt => IsRecurringTransactionActive(rt)).ToList();
            var inactiveTransactions = recurringTransactions.Where(rt => !IsRecurringTransactionActive(rt)).ToList();

            // Calculate monthly amounts
            var monthlyIncomeAmount = activeTransactions
                .Where(rt => rt.Type == "Income")
                .Sum(rt => ConvertToMonthlyAmount(rt.Amount, rt.Frequency));

            var monthlyExpenseAmount = activeTransactions
                .Where(rt => rt.Type == "Expense")
                .Sum(rt => ConvertToMonthlyAmount(rt.Amount, rt.Frequency));

            // Get upcoming transactions (next 7 days)
            var upcomingTransactions = activeTransactions
                .Select(rt => new RecurringTransactionListDto
                {
                    Id = rt.Id,
                    Type = rt.Type,
                    CategoryName = rt.Category.Name,
                    Amount = rt.Amount,
                    Frequency = rt.Frequency,
                    StartDate = rt.StartDate,
                    EndDate = rt.EndDate,
                    AutoDeduction = rt.AutoDeduction,
                    NextExecutionDate = CalculateNextExecutionDate(rt),
                    IsActive = true,
                    GeneratedTransactionsCount = _context.Transactions.Count(t => t.RecurringTransactionId == rt.Id)
                })
                .Where(rt => rt.NextExecutionDate.HasValue && rt.NextExecutionDate.Value <= DateTime.Today.AddDays(7))
                .OrderBy(rt => rt.NextExecutionDate)
                .ToList();

            return new RecurringTransactionSummaryDto
            {
                TotalActive = activeTransactions.Count,
                TotalInactive = inactiveTransactions.Count,
                MonthlyIncomeAmount = monthlyIncomeAmount,
                MonthlyExpenseAmount = monthlyExpenseAmount,
                UpcomingTransactions = upcomingTransactions
            };
        }

        private decimal ConvertToMonthlyAmount(decimal amount, string frequency)
        {
            return frequency switch
            {
                "Daily" => amount * 30,
                "Weekly" => amount * 4.33m,
                "Monthly" => amount,
                "Yearly" => amount / 12,
                _ => 0
            };
        }

        private DateTime? CalculateNextExecutionDate(RecurringTransaction recurringTransaction)
        {
            if (!recurringTransaction.StartDate.HasValue)
                return null;

            var startDate = recurringTransaction.StartDate.Value;
            var today = DateTime.Today;
            DateTime nextDate = startDate;

            if (startDate > today)
                return startDate;

            switch (recurringTransaction.Frequency)
            {
                case "Daily":
                    while (nextDate <= today)
                    {
                        nextDate = nextDate.AddDays(1);
                    }
                    break;
                case "Weekly":
                    while (nextDate <= today)
                    {
                        nextDate = nextDate.AddDays(7);
                    }
                    break;
                case "Monthly":
                    while (nextDate <= today)
                    {
                        nextDate = nextDate.AddMonths(1);
                    }
                    break;
                case "Yearly":
                    while (nextDate <= today)
                    {
                        nextDate = nextDate.AddYears(1);
                    }
                    break;
                default:
                    return null;
            }

            // Check if next execution date is beyond end date
            if (recurringTransaction.EndDate.HasValue && nextDate > recurringTransaction.EndDate.Value)
                return null;

            var executedCount = _context.Transactions.Count(t => t.RecurringTransactionId == recurringTransaction.Id);

            // Check if occurrences limit has been reached
            if (recurringTransaction.Occurrences.HasValue && executedCount >= recurringTransaction.Occurrences.Value)
                return null;

            return nextDate;
        }

        private bool IsRecurringTransactionActive(RecurringTransaction recurringTransaction)
        {
            var today = DateTime.Today;

            // Check if start date is in the future
            if (recurringTransaction.StartDate > today)
                return true;

            // Check if end date has passed
            if (recurringTransaction.EndDate.HasValue && recurringTransaction.EndDate.Value < today)
                return false;

            // Check if occurrences limit has been reached
            if (recurringTransaction.Occurrences.HasValue)
            {
                var transactionCount = _context.Transactions
                    .Count(t => t.RecurringTransactionId == recurringTransaction.Id);

                if (transactionCount >= recurringTransaction.Occurrences.Value)
                    return false;
            }

            return true;
        }
    }
}
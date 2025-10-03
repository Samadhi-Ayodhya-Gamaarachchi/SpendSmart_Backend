using Microsoft.EntityFrameworkCore;
using SpendSmart_Backend.Data;
using SpendSmart_Backend.DTOs;
using SpendSmart_Backend.Models;

namespace SpendSmart_Backend.Services
{
    public interface IRecurringTransactionService
    {
        Task<RecurringTransactionDto> CreateRecurringTransactionAsync(CreateRecurringTransactionDto dto);
        Task<RecurringTransactionDto> GetRecurringTransactionByIdAsync(int id);
        Task<List<RecurringTransactionDto>> GetRecurringTransactionAsync();
        //Task<List<RecurringTransactionListDto>> GetUserRecurringTransactionsAsync(int userId);
        //Task<RecurringTransactionDto> UpdateRecurringTransactionAsync(int id, UpdateRecurringTransactionDto dto);
        Task<bool> DeleteRecurringTransactionAsync(int id);
        Task ProcessRecurringTransactionsAsync();
        Task<List<RecurringTransactionListDto>> GetActiveRecurringTransactionsAsync();
        Task<List<Transaction>> GetTransactionsFromRecurringTransactionAsync(int recurringTransactionId);
        Task<bool> DeleteTransactionFromRecurringAsync(int transactionId);
        Task<RecurringTransactionSummaryDto> GetRecurringTransactionSummaryAsync();
    }
}

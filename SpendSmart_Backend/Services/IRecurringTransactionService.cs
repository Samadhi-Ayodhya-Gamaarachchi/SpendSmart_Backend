using Microsoft.EntityFrameworkCore;
using SpendSmart_Backend.Data;
using SpendSmart_Backend.DTOs;
using SpendSmart_Backend.Models;

namespace SpendSmart_Backend.Services
{
    public interface IRecurringTransactionService
    {
        Task<RecurringTransactionDto> CreateRecurringTransactionAsync(int userId, CreateRecurringTransactionDto dto);
        Task<RecurringTransactionDto> GetRecurringTransactionByIdAsync(int userId,int id);
        Task<List<RecurringTransactionDto>> GetRecurringTransactionAsync(int userId);
        //Task<List<RecurringTransactionListDto>> GetUserRecurringTransactionsAsync(int userId);
        //Task<RecurringTransactionDto> UpdateRecurringTransactionAsync(int id, UpdateRecurringTransactionDto dto);
        Task<bool> DeleteRecurringTransactionAsync(int userId,int id);
        Task ProcessRecurringTransactionsAsync();
        Task<List<RecurringTransactionListDto>> GetActiveRecurringTransactionsAsync(int userId);
        Task<List<TransactionViewDto>> GetTransactionsFromRecurringTransactionAsync(int userId,int recurringTransactionId);
        //Task<bool> DeleteTransactionFromRecurringAsync(int transactionId);
        //Task<RecurringTransactionSummaryDto> GetRecurringTransactionSummaryAsync();
        Task<bool> DeleteAllTransactionsFromRecurringAsync(int userId,int recurringTransactionId);
    }
}

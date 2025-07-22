using SpendSmart_Backend.DTOs;
using SpendSmart_Backend.Models;

namespace SpendSmart_Backend.Services
{
    public interface ITransactionService
    {
        Task<Transaction> CreateTransactionAsync(int userId, TransactionDto dto);
        Task<List<TransactionViewDto>> GetTransactionAsync(
            int userId,
            string? type = null, 
            string? category = null, 
            DateTime? date = null, 
            DateTime? startDate = null,
            DateTime? endDate = null,
            string? sorting = null);
        Task<bool> DeleteTransactionAsync(int userId, int transactionId);
    }
}

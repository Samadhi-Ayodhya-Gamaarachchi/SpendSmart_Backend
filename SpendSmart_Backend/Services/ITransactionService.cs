using SpendSmart_Backend.DTOs;
using SpendSmart_Backend.Models;

namespace SpendSmart_Backend.Services
{
    public interface ITransactionService
    {
        Task<Transaction> CreateTransactionAsync(TransactionDto dto);
        Task<List<TransactionViewDto>> GetTransactionAsync(
            string? type = null, 
            string? category = null, 
            DateTime? date = null, 
            DateTime? startDate = null,
            DateTime? endDate = null,
            string? sorting = null);
        Task<bool> DeleteTransactionAsync(int transactionId);
    }
}

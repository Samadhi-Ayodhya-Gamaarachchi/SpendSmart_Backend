// Services/IBudgetService.cs
using SpendSmart_Backend.DTOs;

namespace SpendSmart_Backend.Services
{
    public interface IBudgetService
    {
        Task<List<BudgetImpactDto>> ProcessBudgetImpactAsync(int userId, int categoryId, decimal amount, DateTime transactionDate, int transactionId);
        Task ReverseBudgetImpactAsync(int transactionId);
        Task RecalculateBudgetAmountsAsync(int budgetId);
        Task<bool> IsBudgetOverLimitAsync(int budgetId, int categoryId);
    }
}
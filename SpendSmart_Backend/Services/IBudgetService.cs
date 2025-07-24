using SpendSmart_Backend.DTOs;

namespace SpendSmart_Backend.Services
{
    public interface IBudgetService
    {
        Task<BudgetSummaryDto> CreateBudgetAsync(int userId, CreateBudgetDto dto);
        Task<BudgetSummaryDto> GetBudgetSummaryByIdAsync(int userId, int id);
        Task<List<BudgetListDto>> GetUserBudgetsAsync(int userId);
        Task<BudgetOverviewDto> GetBudgetOverviewAsync(int userId, DateTime monthYear);
        Task<List<BudgetListDto>> GetBudgetsByMonthAsync(int userId, DateTime monthYear);
        Task<BudgetSummaryDto> UpdateBudgetAsync(int userId, int id, UpdateBudgetDto dto);
        Task<bool> DeleteBudgetAsync(int userId, int id);
        Task<BudgetSummaryDto> AddExpenseToBudgetAsync(int userId, int id, AddExpenseToBudgetDto dto);
        Task<List<BudgetListDto>> GetOverBudgetItemsAsync(int userId, DateTime? monthYear = null);
        Task UpdateBudgetSpendFromTransactionsAsync(int userId, int categoryId, DateTime monthYear);
    }
}
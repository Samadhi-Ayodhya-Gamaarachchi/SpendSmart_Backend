using SpendSmart_Backend.DTOs;

namespace SpendSmart_Backend.Services
{
    public interface IDashboardService
    {
        Task<DashboardSummaryDto> GetDashboardSummary(int userId);
        Task<List<DashboardIncomeExpenseDto>> GetIncomeVsExpenseSummary(int userId, string period);
        Task<List<DashboardPiechartDto>> GetPiechartData(int userId);
    }

}


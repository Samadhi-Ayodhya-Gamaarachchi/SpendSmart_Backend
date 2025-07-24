using SpendSmart_Backend.DTOs;

namespace SpendSmart_Backend.Services
{
    public interface IGoalService
    {
        Task<GoalSummaryDto> CreateGoalAsync(int userId, CreateGoalDto dto);
        Task<GoalSummaryDto> GetGoalSummaryByIdAsync(int userId, int id);
        Task<List<GoalListDto>> GetUserGoalsAsync(int userId);
        Task<GoalSummaryDto> UpdateGoalAsync(int userId, int id, UpdateGoalDto dto);
        Task<bool> DeleteGoalAsync(int userId, int id);
        Task<GoalSummaryDto> AddAmountToGoalAsync(int userId, int id, decimal amount);
        Task<List<GoalListDto>> GetActiveGoalsAsync(int userId);
        Task<List<GoalListDto>> GetAchievedGoalsAsync(int userId);
        Task<SavingsSummaryDto> GetTotalSavingsAsync(int userId);
        Task<decimal> GetTotalSavingsAmountAsync(int userId);
    }
}
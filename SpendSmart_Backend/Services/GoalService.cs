using Microsoft.EntityFrameworkCore;
using SpendSmart_Backend.Data;
using SpendSmart_Backend.DTOs;
using SpendSmart_Backend.Models;

namespace SpendSmart_Backend.Services
{
    public class GoalService : IGoalService
    {
        private readonly ApplicationDbContext _context;

        public GoalService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<GoalSummaryDto> CreateGoalAsync(int userId, CreateGoalDto dto)
        {
            // Validate dates
            if (dto.EndDate <= dto.StartDate)
            {
                throw new ArgumentException("End date must be after start date");
            }

            if (dto.StartDate < DateTime.Today)
            {
                throw new ArgumentException("Start date cannot be in the past");
            }

            var goal = new Goal
            {
                Name = dto.Name,
                TargetAmount = dto.TargetAmount,
                CurrentAmount = dto.CurrentAmount,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Description = dto.Description ?? string.Empty,
                UserId = userId
            };

            _context.Goals.Add(goal);
            await _context.SaveChangesAsync();

            return await GetGoalSummaryByIdAsync(userId, goal.Id);
        }

        public async Task<GoalSummaryDto> GetGoalSummaryByIdAsync(int userId, int id)
        {
            var goal = await _context.Goals
                .FirstOrDefaultAsync(g => g.Id == id && g.UserId == userId);

            if (goal == null)
            {
                throw new ArgumentException("Goal not found");
            }

            return CreateGoalSummaryDto(goal);
        }

        public async Task<List<GoalListDto>> GetUserGoalsAsync(int userId)
        {
            var goals = await _context.Goals
                .Where(g => g.UserId == userId)
                .OrderBy(g => g.EndDate)
                .ToListAsync();

            return goals.Select(g => CreateGoalListDto(g)).ToList();
        }

        public async Task<GoalSummaryDto> UpdateGoalAsync(int userId, int id, UpdateGoalDto dto)
        {
            var goal = await _context.Goals
                .FirstOrDefaultAsync(g => g.Id == id && g.UserId == userId);

            if (goal == null)
            {
                throw new ArgumentException("Goal not found");
            }

            // Update fields if provided
            if (!string.IsNullOrEmpty(dto.Name))
            {
                goal.Name = dto.Name;
            }

            if (dto.TargetAmount.HasValue)
            {
                goal.TargetAmount = dto.TargetAmount.Value;
            }

            if (dto.CurrentAmount.HasValue)
            {
                goal.CurrentAmount = dto.CurrentAmount.Value;
            }

            if (dto.StartDate.HasValue)
            {
                if (dto.StartDate.Value < DateTime.Today)
                {
                    throw new ArgumentException("Start date cannot be in the past");
                }
                goal.StartDate = dto.StartDate.Value;
            }

            if (dto.EndDate.HasValue)
            {
                if (dto.EndDate.Value <= goal.StartDate)
                {
                    throw new ArgumentException("End date must be after start date");
                }
                goal.EndDate = dto.EndDate.Value;
            }

            if (dto.Description != null)
            {
                goal.Description = dto.Description;
            }

            await _context.SaveChangesAsync();

            return CreateGoalSummaryDto(goal);
        }

        public async Task<bool> DeleteGoalAsync(int userId, int id)
        {
            var goal = await _context.Goals
                .FirstOrDefaultAsync(g => g.Id == id && g.UserId == userId);

            if (goal == null)
            {
                return false;
            }

            _context.Goals.Remove(goal);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<GoalSummaryDto> AddAmountToGoalAsync(int userId, int id, decimal amount)
        {
            if (amount <= 0)
            {
                throw new ArgumentException("Amount must be greater than zero");
            }

            var goal = await _context.Goals
                .FirstOrDefaultAsync(g => g.Id == id && g.UserId == userId);

            if (goal == null)
            {
                throw new ArgumentException("Goal not found");
            }

            goal.CurrentAmount += amount;
            await _context.SaveChangesAsync();

            return CreateGoalSummaryDto(goal);
        }

        public async Task<List<GoalListDto>> GetActiveGoalsAsync(int userId)
        {
            var goals = await _context.Goals
                .Where(g => g.UserId == userId && g.EndDate >= DateTime.Today && g.CurrentAmount < g.TargetAmount)
                .OrderBy(g => g.EndDate)
                .ToListAsync();

            return goals.Select(g => CreateGoalListDto(g)).ToList();
        }

        public async Task<List<GoalListDto>> GetAchievedGoalsAsync(int userId)
        {
            var goals = await _context.Goals
                .Where(g => g.UserId == userId && g.CurrentAmount >= g.TargetAmount)
                .OrderByDescending(g => g.EndDate)
                .ToListAsync();

            return goals.Select(g => CreateGoalListDto(g)).ToList();
        }

        public async Task<SavingsSummaryDto> GetTotalSavingsAsync(int userId)
        {
            var activeGoals = await _context.Goals
                .Where(g => g.UserId == userId && g.EndDate >= DateTime.Today && g.CurrentAmount < g.TargetAmount)
                .OrderBy(g => g.EndDate)
                .ToListAsync();

            if (!activeGoals.Any())
            {
                return new SavingsSummaryDto
                {
                    TotalSavings = 0,
                    TotalTargetAmount = 0,
                    OverallSavingsPercentage = 0,
                    ActiveGoalsCount = 0,
                    TotalRemainingAmount = 0,
                    ActiveGoals = new List<GoalListDto>()
                };
            }

            var totalSavings = activeGoals.Sum(g => g.CurrentAmount);
            var totalTargetAmount = activeGoals.Sum(g => g.TargetAmount);
            var totalRemainingAmount = totalTargetAmount - totalSavings;
            var overallSavingsPercentage = totalTargetAmount > 0 ? (totalSavings / totalTargetAmount) * 100 : 0;

            return new SavingsSummaryDto
            {
                TotalSavings = totalSavings,
                TotalTargetAmount = totalTargetAmount,
                OverallSavingsPercentage = Math.Round(overallSavingsPercentage, 2),
                ActiveGoalsCount = activeGoals.Count,
                TotalRemainingAmount = Math.Max(0, totalRemainingAmount),
                ActiveGoals = activeGoals.Select(g => CreateGoalListDto(g)).ToList()
            };
        }

        public async Task<decimal> GetTotalSavingsAmountAsync(int userId)
        {
            var totalSavings = await _context.Goals
                .Where(g => g.UserId == userId && g.EndDate >= DateTime.Today && g.CurrentAmount < g.TargetAmount)
                .SumAsync(g => g.CurrentAmount);

            return totalSavings;
        }

        private GoalSummaryDto CreateGoalSummaryDto(Goal goal)
        {
            var savedPercentage = goal.TargetAmount > 0 ? (goal.CurrentAmount / goal.TargetAmount) * 100 : 0;
            var remainingAmount = Math.Max(0, goal.TargetAmount - goal.CurrentAmount);
            var daysRemaining = (goal.EndDate.Date - DateTime.Today).Days;
            var isAchieved = goal.CurrentAmount >= goal.TargetAmount;

            string status;
            if (isAchieved)
            {
                status = "Achieved";
            }
            else if (daysRemaining < 0)
            {
                status = "Overdue";
            }
            else if (daysRemaining <= 7)
            {
                status = "Urgent";
            }
            else
            {
                status = "Active";
            }

            return new GoalSummaryDto
            {
                Id = goal.Id,
                Name = goal.Name,
                EndDate = goal.EndDate,
                CurrentAmount = goal.CurrentAmount,
                TargetAmount = goal.TargetAmount,
                SavedPercentage = Math.Round(savedPercentage, 2),
                RemainingAmount = remainingAmount,
                DaysRemaining = Math.Max(0, daysRemaining),
                IsAchieved = isAchieved,
                Status = status
            };
        }

        private GoalListDto CreateGoalListDto(Goal goal)
        {
            var savedPercentage = goal.TargetAmount > 0 ? (goal.CurrentAmount / goal.TargetAmount) * 100 : 0;
            var daysRemaining = (goal.EndDate.Date - DateTime.Today).Days;
            var isAchieved = goal.CurrentAmount >= goal.TargetAmount;

            string status;
            if (isAchieved)
            {
                status = "Achieved";
            }
            else if (daysRemaining < 0)
            {
                status = "Overdue";
            }
            else if (daysRemaining <= 7)
            {
                status = "Urgent";
            }
            else
            {
                status = "Active";
            }

            return new GoalListDto
            {
                Id = goal.Id,
                Name = goal.Name,
                EndDate = goal.EndDate,
                CurrentAmount = goal.CurrentAmount,
                TargetAmount = goal.TargetAmount,
                SavedPercentage = Math.Round(savedPercentage, 2),
                Status = status,
                DaysRemaining = Math.Max(0, daysRemaining)
            };
        }
    }
}
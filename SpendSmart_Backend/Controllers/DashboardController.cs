using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SpendSmart_Backend.Services;

namespace SpendSmart_Backend.Controllers
{
    [Route("api/[controller]")]
    public class DashboardController : BaseController
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetDashboardSummary()
        {
            try
            {
                // Get user ID from JWT token - users can only see their own dashboard
                var userId = GetCurrentUserId();
                var dashboardSummary = await _dashboardService.GetDashboardSummary(userId);
                return Ok(dashboardSummary);
            }
            catch(UnauthorizedAccessException)
            {
                return Unauthorized("Invalid token or user access");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error fetching Dashboard {ex.Message}");
            }
        }

        [HttpGet("Bargraph/{period}")]
        public async Task<IActionResult> GetIncomeVsExpenseSummary(string period)
        {
            try
            {
                // Get user ID from JWT token - users can only see their own chart data
                var userId = GetCurrentUserId();
                var result = await _dashboardService.GetIncomeVsExpenseSummary(userId, period);
                return Ok(result);
            }
            catch(UnauthorizedAccessException)
            {
                return Unauthorized("Invalid token or user access");
            }
            catch(Exception ex)
            {
                return StatusCode(500, $"Error fetching bargraph data {ex.Message}");
            }
        }

        [HttpGet("Piechart")]
        public async Task<IActionResult> GetPiechartData()
        {
            try
            {
                // Get user ID from JWT token - users can only see their own pie chart data
                var userId = GetCurrentUserId();
                var result = await _dashboardService.GetPiechartData(userId);
                return Ok(result);
            }
            catch(UnauthorizedAccessException)
            {
                return Unauthorized("Invalid token or user access");
            }
            catch(Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }

}

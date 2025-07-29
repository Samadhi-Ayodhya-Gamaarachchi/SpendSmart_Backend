using Microsoft.AspNetCore.Mvc;
using SpendSmart_Backend.Services;

namespace SpendSmart_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : Controller
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetDashboardSummary(int userId)
        {
            try
            {
                var dashboardSummary = await _dashboardService.GetDashboardSummary(userId);
                return Ok(dashboardSummary);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error fetching Dashboard {ex.Message}");
            }
        }

        [HttpGet("Bargraph/{userId}/{period}")]
        public async Task<IActionResult> GetIncomeVsExpenseSummary(int userId, string period)
        {
            try
            {
                var result = await _dashboardService.GetIncomeVsExpenseSummary(userId, period);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error fetching bargraph data {ex.Message}");
            }
        }

        [HttpGet("Piechart/{userId}")]
        public async Task<IActionResult> GetPiechartData(int userId)
        {
            try
            {
                var result = await _dashboardService.GetPiechartData(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
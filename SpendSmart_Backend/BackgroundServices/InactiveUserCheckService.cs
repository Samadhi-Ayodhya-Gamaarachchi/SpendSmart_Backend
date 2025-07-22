using Microsoft.EntityFrameworkCore;
using SpendSmart_Backend.Data;
using SpendSmart_Backend.Services;

namespace SpendSmart_Backend.BackgroundServices
{
    public class InactiveUserCheckService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<InactiveUserCheckService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromHours(24); // Check daily

        public InactiveUserCheckService(IServiceProvider serviceProvider, ILogger<InactiveUserCheckService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Inactive User Check Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CheckInactiveUsers();
                    _logger.LogInformation("Inactive user check completed successfully");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while checking inactive users");
                }

                // Wait for next check
                await Task.Delay(_checkInterval, stoppingToken);
            }
        }

        private async Task CheckInactiveUsers()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

            // Find users who haven't logged in for 30+ days
            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
            
            var inactiveUsers = await context.Users
                .Where(u => u.LastLoginAt == null || u.LastLoginAt < thirtyDaysAgo)
                .Where(u => u.IsActive) // Only check active users
                .CountAsync();

            if (inactiveUsers > 0)
            {
                _logger.LogInformation($"Found {inactiveUsers} inactive users (30+ days)");
                
                // Create notification
                await notificationService.CreateInactiveUsersNotificationAsync(inactiveUsers);
            }
            else
            {
                _logger.LogInformation("No inactive users found");
            }

            // Cleanup expired notifications while we're at it
            await notificationService.DeleteExpiredNotificationsAsync();
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Inactive User Check Service is stopping");
            await base.StopAsync(stoppingToken);
        }
    }
}

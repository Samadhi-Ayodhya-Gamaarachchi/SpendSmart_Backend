using Microsoft.EntityFrameworkCore;
using SpendSmart_Backend.Data;
using SpendSmart_Backend.Models;

namespace SpendSmart_Backend.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(ApplicationDbContext context, ILogger<NotificationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task CreateNotificationAsync(string type, string title, string message, string priority = "Medium", int? relatedUserId = null, string? relatedUserName = null)
        {
            try
            {
                var notification = new Notification
                {
                    Type = type,
                    Title = title,
                    Message = message,
                    Priority = priority,
                    RelatedUserId = relatedUserId,
                    RelatedUserName = relatedUserName,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddDays(30)
                };

                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Notification created: {type} - {title}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to create notification: {type} - {title}");
            }
        }

        public async Task<List<Notification>> GetUnreadNotificationsAsync()
        {
            return await _context.Notifications
                .Where(n => !n.IsRead && n.ExpiresAt > DateTime.UtcNow)
                .OrderByDescending(n => n.CreatedAt)
                .Take(10) // Limit to last 10 unread
                .ToListAsync();
        }

        public async Task<List<Notification>> GetAllNotificationsAsync(int page = 1, int pageSize = 20)
        {
            return await _context.Notifications
                .Where(n => n.ExpiresAt > DateTime.UtcNow) // Only non-expired
                .OrderByDescending(n => n.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetUnreadCountAsync()
        {
            return await _context.Notifications
                .CountAsync(n => !n.IsRead && n.ExpiresAt > DateTime.UtcNow);
        }

        public async Task MarkAsReadAsync(int notificationId)
        {
            try
            {
                var notification = await _context.Notifications.FindAsync(notificationId);
                if (notification != null)
                {
                    notification.IsRead = true;
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Notification {notificationId} marked as read");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to mark notification {notificationId} as read");
            }
        }

        public async Task MarkAllAsReadAsync()
        {
            try
            {
                var unreadNotifications = await _context.Notifications
                    .Where(n => !n.IsRead && n.ExpiresAt > DateTime.UtcNow)
                    .ToListAsync();

                foreach (var notification in unreadNotifications)
                {
                    notification.IsRead = true;
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation($"Marked {unreadNotifications.Count} notifications as read");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to mark all notifications as read");
            }
        }

        public async Task DeleteExpiredNotificationsAsync()
        {
            try
            {
                var expiredNotifications = await _context.Notifications
                    .Where(n => n.ExpiresAt <= DateTime.UtcNow)
                    .ToListAsync();

                if (expiredNotifications.Any())
                {
                    _context.Notifications.RemoveRange(expiredNotifications);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Deleted {expiredNotifications.Count} expired notifications");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete expired notifications");
            }
        }

        public async Task DeleteNotificationAsync(int notificationId)
        {
            try
            {
                var notification = await _context.Notifications.FindAsync(notificationId);
                if (notification != null)
                {
                    _context.Notifications.Remove(notification);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Notification {notificationId} deleted");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to delete notification {notificationId}");
            }
        }

        // Specific notification creators
        public async Task CreateNewUserNotificationAsync(string userName, int userId)
        {
            await CreateNotificationAsync(
                type: "NewUser",
                title: "New User Registration",
                message: $"User '{userName}' has registered and joined the system",
                priority: "Low",
                relatedUserId: userId,
                relatedUserName: userName
            );
        }

        public async Task CreateInactiveUsersNotificationAsync(int inactiveCount)
        {
            await CreateNotificationAsync(
                type: "InactiveUser",
                title: "Inactive Users Alert",
                message: $"{inactiveCount} users haven't logged in for 30+ days",
                priority: "Medium"
            );
        }

        public async Task CreateEmailServiceFailureNotificationAsync(string userEmail, string errorMessage)
        {
            await CreateNotificationAsync(
                type: "EmailServiceFailure",
                title: "Email Service Failure",
                message: $"Failed to send email to {userEmail}: {errorMessage}",
                priority: "High"
            );
        }
    }
}

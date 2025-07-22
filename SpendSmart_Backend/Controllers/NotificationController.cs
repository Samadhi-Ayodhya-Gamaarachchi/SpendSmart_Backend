using Microsoft.AspNetCore.Mvc;
using SpendSmart_Backend.Models;
using SpendSmart_Backend.Services;

namespace SpendSmart_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<NotificationController> _logger;

        public NotificationController(INotificationService notificationService, ILogger<NotificationController> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        // GET: api/Notification/unread - Get unread notifications with count
        [HttpGet("unread")]
        public async Task<ActionResult<object>> GetUnreadNotifications()
        {
            try
            {
                var notifications = await _notificationService.GetUnreadNotificationsAsync();
                var count = await _notificationService.GetUnreadCountAsync();

                return Ok(new
                {
                    count = count,
                    notifications = notifications.Select(n => new
                    {
                        id = n.Id,
                        type = n.Type,
                        title = n.Title,
                        message = n.Message,
                        priority = n.Priority,
                        createdAt = n.CreatedAt,
                        relatedUserId = n.RelatedUserId,
                        relatedUserName = n.RelatedUserName,
                        timeAgo = GetTimeAgo(n.CreatedAt)
                    })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unread notifications");
                return StatusCode(500, new { message = "Failed to retrieve notifications" });
            }
        }

        // GET: api/Notification - Get all notifications with pagination
        [HttpGet]
        public async Task<ActionResult<object>> GetAllNotifications([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var notifications = await _notificationService.GetAllNotificationsAsync(page, pageSize);

                return Ok(new
                {
                    page = page,
                    pageSize = pageSize,
                    notifications = notifications.Select(n => new
                    {
                        id = n.Id,
                        type = n.Type,
                        title = n.Title,
                        message = n.Message,
                        priority = n.Priority,
                        isRead = n.IsRead,
                        createdAt = n.CreatedAt,
                        relatedUserId = n.RelatedUserId,
                        relatedUserName = n.RelatedUserName,
                        timeAgo = GetTimeAgo(n.CreatedAt)
                    })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all notifications");
                return StatusCode(500, new { message = "Failed to retrieve notifications" });
            }
        }

        // PUT: api/Notification/{id}/read - Mark notification as read
        [HttpPut("{id}/read")]
        public async Task<ActionResult> MarkAsRead(int id)
        {
            try
            {
                await _notificationService.MarkAsReadAsync(id);
                return Ok(new { message = "Notification marked as read" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error marking notification {id} as read");
                return StatusCode(500, new { message = "Failed to mark notification as read" });
            }
        }

        // PUT: api/Notification/mark-all-read - Mark all notifications as read
        [HttpPut("mark-all-read")]
        public async Task<ActionResult> MarkAllAsRead()
        {
            try
            {
                await _notificationService.MarkAllAsReadAsync();
                return Ok(new { message = "All notifications marked as read" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking all notifications as read");
                return StatusCode(500, new { message = "Failed to mark all notifications as read" });
            }
        }

        // DELETE: api/Notification/{id} - Delete specific notification
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteNotification(int id)
        {
            try
            {
                await _notificationService.DeleteNotificationAsync(id);
                return Ok(new { message = "Notification deleted" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting notification {id}");
                return StatusCode(500, new { message = "Failed to delete notification" });
            }
        }

        // DELETE: api/Notification/cleanup - Delete expired notifications (Admin only)
        [HttpDelete("cleanup")]
        public async Task<ActionResult> CleanupExpiredNotifications()
        {
            try
            {
                await _notificationService.DeleteExpiredNotificationsAsync();
                return Ok(new { message = "Expired notifications cleaned up" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up expired notifications");
                return StatusCode(500, new { message = "Failed to cleanup expired notifications" });
            }
        }

        // GET: api/Notification/test-notifications - Generate test notifications (for development)
        [HttpPost("test-notifications")]
        public async Task<ActionResult> GenerateTestNotifications()
        {
            try
            {
                // Create some sample notifications for testing
                await _notificationService.CreateNewUserNotificationAsync("testuser1", 1);
                await _notificationService.CreateNewUserNotificationAsync("testuser2", 2);
                await _notificationService.CreateInactiveUsersNotificationAsync(5);
                await _notificationService.CreateEmailServiceFailureNotificationAsync("test@example.com", "SMTP server connection failed");
                
                return Ok(new { message = "Test notifications created successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating test notifications");
                return StatusCode(500, new { message = "Failed to create test notifications" });
            }
        }

        // Helper method to calculate time ago
        private string GetTimeAgo(DateTime dateTime)
        {
            var timeSpan = DateTime.UtcNow - dateTime;

            if (timeSpan.Days > 0)
                return $"{timeSpan.Days} day{(timeSpan.Days > 1 ? "s" : "")} ago";
            else if (timeSpan.Hours > 0)
                return $"{timeSpan.Hours} hour{(timeSpan.Hours > 1 ? "s" : "")} ago";
            else if (timeSpan.Minutes > 0)
                return $"{timeSpan.Minutes} minute{(timeSpan.Minutes > 1 ? "s" : "")} ago";
            else
                return "Just now";
        }
    }
}

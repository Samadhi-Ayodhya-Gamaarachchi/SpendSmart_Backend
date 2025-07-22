using SpendSmart_Backend.Models;

namespace SpendSmart_Backend.Services
{
    public interface INotificationService
    {
        // Create notifications
        Task CreateNotificationAsync(string type, string title, string message, string priority = "Medium", int? relatedUserId = null, string? relatedUserName = null);
        
        // Get notifications
        Task<List<Notification>> GetUnreadNotificationsAsync();
        Task<List<Notification>> GetAllNotificationsAsync(int page = 1, int pageSize = 20);
        Task<int> GetUnreadCountAsync();
        
        // Mark as read
        Task MarkAsReadAsync(int notificationId);
        Task MarkAllAsReadAsync();
        
        // Cleanup
        Task DeleteExpiredNotificationsAsync();
        Task DeleteNotificationAsync(int notificationId);
        
        // Specific notification creators
        Task CreateNewUserNotificationAsync(string userName, int userId);
        Task CreateInactiveUsersNotificationAsync(int inactiveCount);
        Task CreateEmailServiceFailureNotificationAsync(string userEmail, string errorMessage);
    }
}

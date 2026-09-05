using OnlineFeedbackSystem.Models.Entities;
using OnlineFeedbackSystem.Models.ViewModels;

namespace OnlineFeedbackSystem.Repositories
{
    public interface INotificationsRepository
    {
        NotificationIndexViewModel GetNotifications(int? userId, string? filterType = null, bool unreadOnly = false);
        int GetUnreadCount(int? userId);
        bool MarkAsRead(int id, int? userId);
        bool MarkAllAsRead(int? userId);
        bool Delete(int id, int? userId);
        bool ClearAllRead(int? userId);
        int Create(Notifications notification);
        void EnsureTableAndSeed(int? userId);
        void NotifyUser(int userId, string type, string message, int? formId = null);
        void NotifySuperAdmins(string type, string message, int? formId = null);
    }
}

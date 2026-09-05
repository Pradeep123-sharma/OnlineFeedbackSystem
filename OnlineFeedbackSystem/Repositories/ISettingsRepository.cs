using OnlineFeedbackSystem.Models.ViewModels;

namespace OnlineFeedbackSystem.Repositories
{
    public interface ISettingsRepository
    {
        AdminSettingsViewModel GetSettings(int userId);
        bool SaveSettings(int userId, AdminSettingsViewModel model);
        bool UpdateProfile(int userId, string fullName, string email);
        (bool Success, string Message) ChangePassword(int userId, string currentPassword, string newPassword);
        void ResetToDefaults(int userId);
    }
}

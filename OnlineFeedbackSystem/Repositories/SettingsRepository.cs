using Microsoft.Data.SqlClient;
using OnlineFeedbackSystem.Models.Entities;
using OnlineFeedbackSystem.Models.ViewModels;
using OnlineFeedbackSystem.Services;
using OnlineFeedbackSystem.Web.Data;
using System.Data;

namespace OnlineFeedbackSystem.Repositories
{
    public class SettingsRepository : ISettingsRepository
    {
        private readonly DbHelper _dbHelper;
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IDepartmentRepository _departmentRepository;
        private readonly PasswordHasherService _passwordHasher;

        public SettingsRepository(
            DbHelper dbHelper,
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            IDepartmentRepository departmentRepository,
            PasswordHasherService passwordHasher)
        {
            _dbHelper = dbHelper;
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _departmentRepository = departmentRepository;
            _passwordHasher = passwordHasher;
        }

        private void EnsureTableExists()
        {
            try
            {
                using var connection = _dbHelper.CreateOpenConnection();
                string sql = @"
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SystemSettings]') AND type in (N'U'))
                BEGIN
                    CREATE TABLE [dbo].[SystemSettings] (
                        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        [UserId] INT NULL,
                        [EmailOnNewResponse] BIT NOT NULL DEFAULT 1,
                        [LowRatingAlertThreshold] FLOAT NOT NULL DEFAULT 3.0,
                        [DailyDigestEnabled] BIT NOT NULL DEFAULT 0,
                        [NotifyOnFormClosure] BIT NOT NULL DEFAULT 1,
                        [NotifyOnMilestone] BIT NOT NULL DEFAULT 1,
                        [DefaultValidityDays] INT NOT NULL DEFAULT 14,
                        [DefaultIsAnonymous] BIT NOT NULL DEFAULT 0,
                        [DefaultOneResponseOnly] BIT NOT NULL DEFAULT 1,
                        [DefaultWelcomeMessage] NVARCHAR(MAX) NULL DEFAULT 'Thank you for taking the time to share your feedback. Your input helps us continuously improve!',
                        [DefaultThankYouMessage] NVARCHAR(MAX) NULL DEFAULT 'Your response has been recorded successfully. Thank you for your valuable feedback!',
                        [MaxQuestionsPerForm] INT NOT NULL DEFAULT 25,
                        [ExportFormatPreference] NVARCHAR(20) NOT NULL DEFAULT 'Excel',
                        [SessionTimeoutMinutes] INT NOT NULL DEFAULT 480,
                        [UpdatedAt] DATETIME NOT NULL DEFAULT GETDATE()
                    );
                END";
                using var cmd = new SqlCommand(sql, connection);
                cmd.ExecuteNonQuery();
            }
            catch
            {
                // Fallback gracefully
            }
        }

        public AdminSettingsViewModel GetSettings(int userId)
        {
            EnsureTableExists();

            var model = new AdminSettingsViewModel { UserId = userId };

            // 1. Fetch User Info
            var user = _userRepository.GetById(userId);
            if (user != null)
            {
                model.FullName = user.FullName;
                model.Email = user.Email;
                model.MemberSince = user.CreatedAt;

                var role = _roleRepository.GetById(user.RoleId);
                model.RoleName = role?.RoleName ?? "Administrator";

                if (user.DepartmentId.HasValue)
                {
                    var dept = _departmentRepository.GetById(user.DepartmentId.Value);
                    model.DepartmentName = dept?.DepartmentName ?? "All Departments";
                }
                else
                {
                    model.DepartmentName = "Central Administration";
                }
            }

            // 2. Fetch or initialize System/User Settings
            using var connection = _dbHelper.CreateOpenConnection();
            string sql = "SELECT TOP 1 * FROM SystemSettings WHERE (UserId = @UserId) OR (@UserId IS NOT NULL AND UserId IS NULL) ORDER BY CASE WHEN UserId = @UserId THEN 0 ELSE 1 END, Id DESC;";
            using var cmd = new SqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@UserId", userId);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                model.EmailOnNewResponse = reader.GetBoolean(reader.GetOrdinal("EmailOnNewResponse"));
                model.LowRatingAlertThreshold = reader.GetDouble(reader.GetOrdinal("LowRatingAlertThreshold"));
                model.DailyDigestEnabled = reader.GetBoolean(reader.GetOrdinal("DailyDigestEnabled"));
                model.NotifyOnFormClosure = reader.GetBoolean(reader.GetOrdinal("NotifyOnFormClosure"));
                model.NotifyOnMilestone = reader.GetBoolean(reader.GetOrdinal("NotifyOnMilestone"));

                model.DefaultValidityDays = reader.GetInt32(reader.GetOrdinal("DefaultValidityDays"));
                model.DefaultIsAnonymous = reader.GetBoolean(reader.GetOrdinal("DefaultIsAnonymous"));
                model.DefaultOneResponseOnly = reader.GetBoolean(reader.GetOrdinal("DefaultOneResponseOnly"));
                model.DefaultWelcomeMessage = reader.IsDBNull(reader.GetOrdinal("DefaultWelcomeMessage")) ? model.DefaultWelcomeMessage : reader.GetString(reader.GetOrdinal("DefaultWelcomeMessage"));
                model.DefaultThankYouMessage = reader.IsDBNull(reader.GetOrdinal("DefaultThankYouMessage")) ? model.DefaultThankYouMessage : reader.GetString(reader.GetOrdinal("DefaultThankYouMessage"));

                model.MaxQuestionsPerForm = reader.GetInt32(reader.GetOrdinal("MaxQuestionsPerForm"));
                model.ExportFormatPreference = reader.GetString(reader.GetOrdinal("ExportFormatPreference"));
                model.SessionTimeoutMinutes = reader.GetInt32(reader.GetOrdinal("SessionTimeoutMinutes"));
                model.UpdatedAt = reader.GetDateTime(reader.GetOrdinal("UpdatedAt"));
            }

            return model;
        }

        public bool SaveSettings(int userId, AdminSettingsViewModel model)
        {
            EnsureTableExists();

            using var connection = _dbHelper.CreateOpenConnection();
            string sql = @"
            IF EXISTS (SELECT 1 FROM SystemSettings WHERE UserId = @UserId)
            BEGIN
                UPDATE SystemSettings
                SET 
                    EmailOnNewResponse = @EmailOnNewResponse,
                    LowRatingAlertThreshold = @LowRatingAlertThreshold,
                    DailyDigestEnabled = @DailyDigestEnabled,
                    NotifyOnFormClosure = @NotifyOnFormClosure,
                    NotifyOnMilestone = @NotifyOnMilestone,
                    DefaultValidityDays = @DefaultValidityDays,
                    DefaultIsAnonymous = @DefaultIsAnonymous,
                    DefaultOneResponseOnly = @DefaultOneResponseOnly,
                    DefaultWelcomeMessage = @DefaultWelcomeMessage,
                    DefaultThankYouMessage = @DefaultThankYouMessage,
                    MaxQuestionsPerForm = @MaxQuestionsPerForm,
                    ExportFormatPreference = @ExportFormatPreference,
                    SessionTimeoutMinutes = @SessionTimeoutMinutes,
                    UpdatedAt = GETDATE()
                WHERE UserId = @UserId;
            END
            ELSE
            BEGIN
                INSERT INTO SystemSettings (
                    UserId, EmailOnNewResponse, LowRatingAlertThreshold, DailyDigestEnabled, 
                    NotifyOnFormClosure, NotifyOnMilestone, DefaultValidityDays, DefaultIsAnonymous, 
                    DefaultOneResponseOnly, DefaultWelcomeMessage, DefaultThankYouMessage, 
                    MaxQuestionsPerForm, ExportFormatPreference, SessionTimeoutMinutes, UpdatedAt
                )
                VALUES (
                    @UserId, @EmailOnNewResponse, @LowRatingAlertThreshold, @DailyDigestEnabled, 
                    @NotifyOnFormClosure, @NotifyOnMilestone, @DefaultValidityDays, @DefaultIsAnonymous, 
                    @DefaultOneResponseOnly, @DefaultWelcomeMessage, @DefaultThankYouMessage, 
                    @MaxQuestionsPerForm, @ExportFormatPreference, @SessionTimeoutMinutes, GETDATE()
                );
            END";

            using var cmd = new SqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@UserId", userId);
            cmd.Parameters.AddWithValue("@EmailOnNewResponse", model.EmailOnNewResponse);
            cmd.Parameters.AddWithValue("@LowRatingAlertThreshold", model.LowRatingAlertThreshold);
            cmd.Parameters.AddWithValue("@DailyDigestEnabled", model.DailyDigestEnabled);
            cmd.Parameters.AddWithValue("@NotifyOnFormClosure", model.NotifyOnFormClosure);
            cmd.Parameters.AddWithValue("@NotifyOnMilestone", model.NotifyOnMilestone);

            cmd.Parameters.AddWithValue("@DefaultValidityDays", model.DefaultValidityDays);
            cmd.Parameters.AddWithValue("@DefaultIsAnonymous", model.DefaultIsAnonymous);
            cmd.Parameters.AddWithValue("@DefaultOneResponseOnly", model.DefaultOneResponseOnly);
            cmd.Parameters.AddWithValue("@DefaultWelcomeMessage", (object?)model.DefaultWelcomeMessage ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@DefaultThankYouMessage", (object?)model.DefaultThankYouMessage ?? DBNull.Value);

            cmd.Parameters.AddWithValue("@MaxQuestionsPerForm", model.MaxQuestionsPerForm);
            cmd.Parameters.AddWithValue("@ExportFormatPreference", model.ExportFormatPreference ?? "Excel");
            cmd.Parameters.AddWithValue("@SessionTimeoutMinutes", model.SessionTimeoutMinutes);

            return cmd.ExecuteNonQuery() > 0;
        }

        public bool UpdateProfile(int userId, string fullName, string email)
        {
            var user = _userRepository.GetById(userId);
            if (user == null) return false;

            // Check if email changed and is taken by another user
            var existingByEmail = _userRepository.GetByEmail(email.Trim());
            if (existingByEmail != null && existingByEmail.UserId != userId)
            {
                throw new InvalidOperationException("Email address is already in use by another user.");
            }

            user.FullName = fullName.Trim();
            user.Email = email.Trim();

            return _userRepository.Update(user);
        }

        public (bool Success, string Message) ChangePassword(int userId, string currentPassword, string newPassword)
        {
            var user = _userRepository.GetById(userId);
            if (user == null) return (false, "User account not found.");

            // Verify current password
            if (!_passwordHasher.VerifyPassword(user, currentPassword))
            {
                return (false, "The current password entered is incorrect.");
            }

            // Hash new password
            string newHash = _passwordHasher.HashPassword(user, newPassword);
            user.PasswordHash = newHash;

            bool updated = _userRepository.Update(user);
            return updated ? (true, "Password has been changed successfully.") : (false, "Failed to update password. Please try again.");
        }

        public void ResetToDefaults(int userId)
        {
            EnsureTableExists();

            using var connection = _dbHelper.CreateOpenConnection();
            string sql = "DELETE FROM SystemSettings WHERE UserId = @UserId;";
            using var cmd = new SqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@UserId", userId);
            cmd.ExecuteNonQuery();
        }
    }
}

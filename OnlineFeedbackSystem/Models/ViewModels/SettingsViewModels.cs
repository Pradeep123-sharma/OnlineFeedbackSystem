using System.ComponentModel.DataAnnotations;

namespace OnlineFeedbackSystem.Models.ViewModels
{
    public class AdminSettingsViewModel
    {
        public string ActiveTab { get; set; } = "profile"; // profile, notifications, formDefaults, system

        // === 1. Profile & Account Info ===
        public int UserId { get; set; }

        [Required(ErrorMessage = "Full name is required.")]
        [StringLength(100, ErrorMessage = "Full name cannot exceed 100 characters.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string Email { get; set; } = string.Empty;

        public string RoleName { get; set; } = "Admin";
        public string? DepartmentName { get; set; }
        public DateTime MemberSince { get; set; }

        // === 2. Password Change ===
        [DataType(DataType.Password)]
        public string? CurrentPassword { get; set; }

        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "New password must be at least 6 characters long.")]
        public string? NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "New password and confirmation do not match.")]
        public string? ConfirmPassword { get; set; }

        // === 3. Notification Preferences ===
        public bool EmailOnNewResponse { get; set; } = true;

        [Range(1.0, 5.0, ErrorMessage = "Threshold must be between 1.0 and 5.0")]
        public double LowRatingAlertThreshold { get; set; } = 3.0;

        public bool DailyDigestEnabled { get; set; } = false;
        public bool NotifyOnFormClosure { get; set; } = true;
        public bool NotifyOnMilestone { get; set; } = true;

        // === 4. Form Defaults ===
        [Required]
        [Range(1, 365, ErrorMessage = "Default validity must be between 1 and 365 days.")]
        public int DefaultValidityDays { get; set; } = 14;

        public bool DefaultIsAnonymous { get; set; } = false;
        public bool DefaultOneResponseOnly { get; set; } = true;

        [StringLength(1000, ErrorMessage = "Welcome message cannot exceed 1000 characters.")]
        public string DefaultWelcomeMessage { get; set; } = "Thank you for taking the time to share your feedback. Your input helps us continuously improve!";

        [StringLength(1000, ErrorMessage = "Thank you message cannot exceed 1000 characters.")]
        public string DefaultThankYouMessage { get; set; } = "Your response has been recorded successfully. Thank you for your valuable feedback!";

        // === 5. System Preferences ===
        [Range(1, 100, ErrorMessage = "Max questions must be between 1 and 100.")]
        public int MaxQuestionsPerForm { get; set; } = 25;

        public string ExportFormatPreference { get; set; } = "Excel"; // Excel, PDF, CSV

        [Range(15, 1440, ErrorMessage = "Session timeout must be between 15 and 1440 minutes.")]
        public int SessionTimeoutMinutes { get; set; } = 480;

        public DateTime UpdatedAt { get; set; }
    }
}

namespace OnlineFeedbackSystem.Models.Entities
{
    public class SystemSettings
    {
        public int Id { get; set; }
        public int? UserId { get; set; }

        // Notification Preferences
        public bool EmailOnNewResponse { get; set; } = true;
        public double LowRatingAlertThreshold { get; set; } = 3.0;
        public bool DailyDigestEnabled { get; set; } = false;
        public bool NotifyOnFormClosure { get; set; } = true;
        public bool NotifyOnMilestone { get; set; } = true;

        // Form Defaults
        public int DefaultValidityDays { get; set; } = 14;
        public bool DefaultIsAnonymous { get; set; } = false;
        public bool DefaultOneResponseOnly { get; set; } = true;
        public string DefaultWelcomeMessage { get; set; } = "Thank you for taking the time to share your feedback. Your input helps us continuously improve!";
        public string DefaultThankYouMessage { get; set; } = "Your response has been recorded successfully. Thank you for your valuable feedback!";

        // System Preferences
        public int MaxQuestionsPerForm { get; set; } = 25;
        public string ExportFormatPreference { get; set; } = "Excel"; // Excel, PDF, CSV
        public int SessionTimeoutMinutes { get; set; } = 480;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}

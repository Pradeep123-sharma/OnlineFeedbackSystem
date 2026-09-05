namespace OnlineFeedbackSystem.Models.ViewModels
{
    public class NotificationItemViewModel
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public int? FormId { get; set; }
        public string? FormTitle { get; set; }
        public string Type { get; set; } = "System"; // Submission, LowRating, Publication, Closure, System
        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }

        public string TimeAgo
        {
            get
            {
                var span = DateTime.Now - CreatedAt;
                if (span.TotalMinutes < 1) return "Just now";
                if (span.TotalMinutes < 60) return $"{(int)span.TotalMinutes}m ago";
                if (span.TotalHours < 24) return $"{(int)span.TotalHours}h ago";
                if (span.TotalDays < 7) return $"{(int)span.TotalDays}d ago";
                return CreatedAt.ToString("MMM dd, yyyy");
            }
        }

        public string TypeEmoji
        {
            get => Type switch
            {
                "Submission" => "\U0001F4AC", // 💬
                "LowRating" => "\U000026A0\U0000FE0F", // ⚠️
                "Publication" => "\U000026A1", // ⚡
                "Closure" => "\U0001F512", // 🔒
                "Milestone" => "\U0001F389", // 🎉
                _ => "\U0001F514" // 🔔
            };
        }

        public string IconColorClass
        {
            get => Type switch
            {
                "Submission" => "icon-sky",
                "LowRating" => "icon-coral",
                "Publication" => "icon-mint",
                "Closure" => "icon-sun",
                "Milestone" => "icon-grape",
                _ => "icon-sky"
            };
        }
    }

    public class NotificationIndexViewModel
    {
        public List<NotificationItemViewModel> Notifications { get; set; } = new();
        public string SelectedFilter { get; set; } = "All"; // All, Unread, Submission, LowRating, Publication, System
        public int TotalCount { get; set; }
        public int UnreadCount { get; set; }
        public int AlertCount { get; set; }
        public int SubmissionCount { get; set; }
    }
}

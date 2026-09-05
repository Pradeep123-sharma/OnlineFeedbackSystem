namespace OnlineFeedbackSystem.Models.ViewModels
{
    public class AnalyticsDashboardIndexViewModel
    {
        public List<FeedbackFormListItemViewModel> Forms { get; set; } = new();
    }

    public class FormAnalyticsViewModel
    {
        public int FormId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public int TotalResponses { get; set; }
        public double OverallAverageRating { get; set; }
        public int QuestionCount { get; set; }

        // Question-wise statistics
        public List<QuestionAnalyticsItemViewModel> QuestionAnalytics { get; set; } = new();

        // Low rating alerts (ratings below threshold e.g. < 3.0)
        public List<LowRatingAlertViewModel> LowRatingAlerts { get; set; } = new();

        // Textual suggestions & comments
        public List<CommentFeedbackViewModel> TextComments { get; set; } = new();
    }

    public class QuestionAnalyticsItemViewModel
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public string QuestionType { get; set; } = string.Empty;
        public int TotalAnswers { get; set; }
        public double? AverageScore { get; set; }

        // Breakdown for choices (Option Text -> count & percentage)
        public List<OptionBreakdownItemViewModel> OptionBreakdown { get; set; } = new();
    }

    public class OptionBreakdownItemViewModel
    {
        public string OptionText { get; set; } = string.Empty;
        public int Count { get; set; }
        public double Percentage { get; set; }
    }

    public class LowRatingAlertViewModel
    {
        public string QuestionText { get; set; } = string.Empty;
        public double RatingValue { get; set; }
        public string RespondentInfo { get; set; } = string.Empty;
        public DateTime SubmittedAt { get; set; }
    }

    public class CommentFeedbackViewModel
    {
        public string QuestionText { get; set; } = string.Empty;
        public string Comment { get; set; } = string.Empty;
        public string RespondentInfo { get; set; } = string.Empty;
        public DateTime SubmittedAt { get; set; }
    }
}

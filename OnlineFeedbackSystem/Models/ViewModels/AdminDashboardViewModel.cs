namespace OnlineFeedbackSystem.Models.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalForms { get; set; }
        public int ActiveForms { get; set; }
        public int TotalResponses { get; set; }
        public double AverageRating { get; set; }
        public int TotalQuestionsInBank { get; set; }
        public List<AdminRecentFormItemViewModel> RecentForms { get; set; } = new();
    }

    public class AdminRecentFormItemViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int ResponseCount { get; set; }
        public int QuestionCount { get; set; }
    }
}

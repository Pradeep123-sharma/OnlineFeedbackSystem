namespace OnlineFeedbackSystem.Models.ViewModels
{
    // Everything the Super Admin dashboard view needs, pre-shaped.
    // Notice this is a plain data holder — no logic — the repository
    // does the calculating, this just carries the results to the view.
    public class DashboardStatsViewModel
    {
        public int TotalForms { get; set; }
        public int ActiveForms { get; set; }
        public int TotalResponses { get; set; }
        public double AverageRating { get; set; }
        public int TotalUsers { get; set; }
        public int TotalDepartments { get; set; }

        public List<RecentFormViewModel> RecentForms { get; set; } = new();
        public List<RecentUserViewModel> RecentUsers { get; set; } = new();
    }

    public class RecentFormViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int ResponseCount { get; set; }
    }

    public class RecentUserViewModel
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public string? DepartmentName { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

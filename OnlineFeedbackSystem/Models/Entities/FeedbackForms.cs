namespace OnlineFeedbackSystem.Models.Entities
{
    public class FeedbackForms
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Category { get; set; } = string.Empty;
        public DateTime StartDate { get; set; } = DateTime.Today;
        public DateTime EndDate { get; set; } = DateTime.Today.AddDays(14);
        public bool IsAnonymous { get; set; } = false;
        public bool OneResponseOnly { get; set; } = true;
        public string Status { get; set; } = "Draft"; // Draft, Published, Closed, Archived
        public int CreatedBy { get; set; }
        public int? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }

        // Navigation collections
        public List<FormQuestions> FormQuestions { get; set; } = new();
    }
}

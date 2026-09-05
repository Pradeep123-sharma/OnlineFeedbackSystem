namespace OnlineFeedbackSystem.Models.Entities
{
    public class Notifications
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public int? FormId { get; set; }
        public string Type { get; set; } = "System"; // Submission, LowRating, Publication, Closure, System
        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation / joined property
        public string? FormTitle { get; set; }
    }
}

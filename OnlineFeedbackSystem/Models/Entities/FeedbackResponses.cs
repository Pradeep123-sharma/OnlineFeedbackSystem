namespace OnlineFeedbackSystem.Models.Entities
{
    public class FeedbackResponses
    {
        public int Id { get; set; }
        public int FormId { get; set; }
        public int? UserId { get; set; }
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "Submitted";
        public string SubmissionToken { get; set; } = string.Empty;

        // Navigation collections
        public List<FeedbackAnswers> Answers { get; set; } = new();
    }
}

namespace OnlineFeedbackSystem.Models.Entities
{
    public class AuditLogs
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public string Action { get; set; } = string.Empty;
        public string? EntityName { get; set; }
        public int? EntityId { get; set; }
        public string? Details { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Joined properties for rich UI display
        public string? UserName { get; set; }
        public string? UserEmail { get; set; }
        public string? RoleName { get; set; }
    }
}

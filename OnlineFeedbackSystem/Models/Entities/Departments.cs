namespace OnlineFeedbackSystem.Models.Entities
{
    public class Departments
    {
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }
}

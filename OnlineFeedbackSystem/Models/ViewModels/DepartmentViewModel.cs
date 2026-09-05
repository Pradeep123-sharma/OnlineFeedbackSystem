using System.ComponentModel.DataAnnotations;

namespace OnlineFeedbackSystem.Models.ViewModels
{
    public class DepartmentViewModel
    {
        
        public int DepartmentId { get; set; }

        [Required(ErrorMessage = "Department name is required.")]
        [StringLength(150, ErrorMessage = "Department name cannot exceed 150 characters.")]
        [Display(Name = "Department Name")]
        public string DepartmentName { get; set; } = string.Empty;

        public bool IsActive { get; set; }

    }
}

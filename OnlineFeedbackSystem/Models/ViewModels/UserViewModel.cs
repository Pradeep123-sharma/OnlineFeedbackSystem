using System.ComponentModel.DataAnnotations;

namespace OnlineFeedbackSystem.Models.ViewModels
{
    public class UserViewModel
    {
        public int UserId { get; set; }

        [Required(ErrorMessage = "Full name is required.")]
        [MaxLength(150)]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        public string Email { get; set; } = string.Empty;

        // Plain text here on purpose — this is what the admin types into
        // the form. The controller hashes it before it ever reaches
        // UserRepository; Users.PasswordHash (the entity) never sees the
        // plain value.
        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Select a role.")]
        public int RoleId { get; set; }

        // No [Required] — matches Users.DepartmentId being nullable.
        // A Super Admin, for instance, legitimately has no department.
        public int? DepartmentId { get; set; }

        public bool IsActive { get; set; } = true;

        // Populated by the controller before the view renders, same
        // pattern as DepartmentFormViewModel.AvailableManagers was —
        // these fill the two <select> dropdowns, they're not part of
        // what gets posted back.
        public List<RoleOptionViewModel> AvailableRoles { get; set; } = new();
        public List<DepartmentOptionViewModel> AvailableDepartments { get; set; } = new();
    }

    public class RoleOptionViewModel
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
    }

    public class DepartmentOptionViewModel
    {
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
    }

    // Shaped specifically for the Index table — RoleName/DepartmentName
    // resolved to display text, not the raw RoleId/DepartmentId a form
    // would need. Separate from UserViewModel because a list row and a
    // form need different shapes, same reasoning as Question's
    // OptionCount-only-on-list-not-on-form split from earlier.
    public class UserListItemViewModel
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}

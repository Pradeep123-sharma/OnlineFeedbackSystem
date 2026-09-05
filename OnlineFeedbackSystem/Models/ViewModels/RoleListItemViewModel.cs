namespace OnlineFeedbackSystem.Models.ViewModels
{
    public class RoleListItemViewModel
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;

        // Not stored anywhere — your schema has no Permissions table, so
        // this is pulled from the doc's Section 3 responsibilities table
        // and matched by RoleName in the controller. Purely descriptive;
        // it has no effect on what a role can actually do. The real
        // enforcement is the [Authorize(Roles = "...")] attributes on
        // each controller — this text just explains what those attributes
        // are protecting, in one place, for whoever's looking at this screen.
        public string Responsibility { get; set; } = string.Empty;
    }
}
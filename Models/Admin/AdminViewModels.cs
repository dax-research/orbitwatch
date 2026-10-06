using System.ComponentModel.DataAnnotations;

namespace OrbitWatch.Models.Admin
{
    public class UserViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string CurrentRole { get; set; } = string.Empty;
    }

    public class EditRoleViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        [Required]
        public string SelectedRole { get; set; } = string.Empty;
        public List<string> AvailableRoles { get; set; } = new List<string>();
    }
}

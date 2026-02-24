using System.Collections.Generic;

namespace DeviceManager.Models
{
    public sealed class EditUserRolesViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string? Email { get; set; }
        public List<RoleSelection> Roles { get; set; } = [];
    }

   

}

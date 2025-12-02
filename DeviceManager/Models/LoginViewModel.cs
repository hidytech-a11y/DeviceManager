using System.ComponentModel.DataAnnotations;

namespace DeviceManager.Models
{
    public sealed class LoginViewModel
    {
        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        public bool RememberMe { get; set; }

        // configured cookie uses "returnUrl" — match that parameter name
        public string? ReturnUrl { get; set; }
    }
}
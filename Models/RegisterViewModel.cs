using System.ComponentModel.DataAnnotations;

namespace DeviceManager.Models
{
    public sealed class RegisterViewModel
    {
        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [MinLength(4)]
        public string? Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
        public string? ConfirmPassword { get; set; }

        [Required]
        [Display(Name = "Role")]
        public string? Role { get; set; }

        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public string? Expertise { get; set; }

    }
}

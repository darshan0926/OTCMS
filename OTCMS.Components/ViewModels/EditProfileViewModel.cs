using System.ComponentModel.DataAnnotations;

namespace OTCMS.Components.ViewModels
{
    public class EditProfileViewModel
    {
        // We'll keep the identity to load the correct user in POST
        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required(ErrorMessage = "First name is required")]
        [MaxLength(50, ErrorMessage = "Invalid size")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [MaxLength(50, ErrorMessage = "Invalid size")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mobile number is required")]
        [MaxLength(10, ErrorMessage = "Invalid size")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Mobile must be a 10-digit number")]
        public string Mobile { get; set; } = string.Empty;
    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OTCMS.Components
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "First name is required")]
        [MaxLength(50, ErrorMessage = "Invalid size")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [MaxLength(50, ErrorMessage = "Invalid size")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]         // Hardcodes the @gmail.com domain
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@gmail\.com$", ErrorMessage = "Only Gmail addresses are permitted.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mobile number is required")]
        [MaxLength(10, ErrorMessage = "Invalid size")]
        [MinLength(10, ErrorMessage = "Invalid size")]
        [RegularExpression("^[0-9]{10,10}$", ErrorMessage = "Only Digits are allowed")]
        public string Mobile { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [MaxLength(256, ErrorMessage = "Invalid size")]
        [DataType(DataType.Password)]
        [StringLength(256, MinimumLength = 6, ErrorMessage = "Password must be 6 or more characters")]

        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "User id is required")]
        [MaxLength(50, ErrorMessage = "Invalid size")]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public bool Status { get; set; }

        [Required(ErrorMessage = "Confirm Password is required")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Password and Confirm Password do not match")]
        [NotMapped] // ensures EF does not create a column in the database
        public string ConfirmPassword { get; set; } = string.Empty;

        //foreign key
        public int RoleId { get; set; }

        //navigation property - one user is mapped to only one role
        public Role? role { get; set; }

        public ICollection<Payment> payments { get; set; } = new List<Payment>();

    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OTCMS.Components
{
    public class Role
    {
        [Key]

        [DatabaseGenerated(DatabaseGeneratedOption.None)]

        public int RoleId { get; set; }

        [Required(ErrorMessage = "Empty value is not allowed")]

        [MaxLength(15, ErrorMessage = "Invalid Role name size")]

        public string RoleName { get; set; } = string.Empty;

        public List<User> users { get; set; }

    }
}

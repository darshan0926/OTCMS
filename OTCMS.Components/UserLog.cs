using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OTCMS.Components
{
    public class UserLog
    {
        [Key]
        public int LogId { get; set; }   // Identity PK

        [ForeignKey("User")]
        [Required(ErrorMessage = "User Id is required")]
        public int Id { get; set; }      // FK to User.Id

        [Required]
        public DateTime LoginTime { get; set; } = DateTime.Now;

        public DateTime? LogoutTime { get; set; }


        // Navigation property - each log belongs to one user
        
        public virtual User? User { get; set; }
    }
}
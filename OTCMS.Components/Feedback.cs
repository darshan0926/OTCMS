using System.ComponentModel.DataAnnotations;

using System.ComponentModel.DataAnnotations.Schema;

namespace OTCMS.Components

{

    public enum FbStatus

    {

        Open,

        UnderProcessing,

        Closed

    }

    public class Feedback

    {

        [Key]

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]// Identity column

        public int FeedbackId { get; set; }

        [Required(ErrorMessage = "Empty value is not allowed")]

        [MaxLength(50, ErrorMessage = "Invalid size")]

        [MinLength(3, ErrorMessage = "Invalid size")]

        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Empty value is not allowed")]

        [MaxLength(50, ErrorMessage = "Invalid size")]

        [MinLength(3, ErrorMessage = "Invalid size")]

        public string Subject { get; set; } = string.Empty;

        [Required(ErrorMessage = "Empty value is not allowed")]

        [MaxLength(500, ErrorMessage = "Invalid size")]

        [MinLength(30, ErrorMessage = "Invalid size")]

        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Empty value is not allowed")]

        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "A phone number is required.")]

        [RegularExpression(@"^([0-9]{10})$", ErrorMessage = "Invalid Mobile Number.")]

        [Display(Name = "Phone Number")]

        public string Phone { get; set; } = string.Empty;

        public FbStatus Status { get; set; }

        public string RemarksByAdmin { get; set; } = string.Empty;

        public DateTime CreatedOn { get; set; }

    }

}


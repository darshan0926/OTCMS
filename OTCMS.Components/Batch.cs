using OTCMS.Components;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OTCMS.Components
{
    public class Batch
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [RegularExpression("^[0-9]{1,4}$", ErrorMessage = "BatchId must be numeric with up to 4 digits")]
        public int BatchId { get; set; }

        [Required(ErrorMessage = "Empty value is not allowed")]
        [MaxLength(100, ErrorMessage = "Invalid value")]
        [MinLength(3, ErrorMessage = "Invalid value")]
        public string BatchName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Empty value is not allowed")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; } 

        [Required(ErrorMessage = "Empty value is not allowed")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        [Required(ErrorMessage = "Empty value is not allowed")]
        [MaxLength(100, ErrorMessage = "Invalid value")]
        public string HandledBy { get; set; } = string.Empty;

        [Required]
        public bool Status  { get; set; }

        [Required(ErrorMessage = "Empty value is not allowed")]
        [MaxLength(50, ErrorMessage = "Invalid value")]
        public string Timings { get; set; } = string.Empty;

        [Required(ErrorMessage = "Empty value is not allowed")]
        [MaxLength(50, ErrorMessage = "Invalid value")]
        public string Duration { get; set; } = string.Empty;

        [Required(ErrorMessage = "Empty value is not allowed")]
        [Range(0, 999999.99, ErrorMessage = "Fee must be a valid decimal value")]
        public decimal Fee { get; set; }

        [Required(ErrorMessage = "Empty value is not allowed")]
        [MaxLength(500, ErrorMessage = "Invalid value")]
        [MinLength(20, ErrorMessage = "Invalid value")]
        public string Description { get; set; } = string.Empty;


        [Required(ErrorMessage = "Empty value is not allowed")]
        [ForeignKey("Course")]
        public int CourseId { get; set; }

        // Navigation Property
        public Course? Course { get; set; }
    }
}

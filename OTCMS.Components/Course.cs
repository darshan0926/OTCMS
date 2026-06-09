using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OTCMS.Components
{
    public class Course
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [RegularExpression("^[0-9]{1,4}$", ErrorMessage = "CourseId must be numeric with up to 4 digits")]
        public int CourseId { get; set; }

        [Required(ErrorMessage = "Empty value is not allowed")]
        [MaxLength(100, ErrorMessage = "Invalid value")]
        [MinLength(1, ErrorMessage = "Invalid Value")]
        public string? NameOfTheCourse { get; set; } = string.Empty;

        [Required(ErrorMessage = "Empty value is not allowed")]
        [MaxLength(500, ErrorMessage = "Invalid value")]
        [MinLength(10, ErrorMessage = "Invalid value")]
        public string CourseDescription { get; set; } = string.Empty;

        [Required]
        public bool Status { get; set; }

        public string CourseLogoImagePath { get; set; } = string.Empty;

        public string? CourseContentFilePath { get; set; } = string.Empty;

        [NotMapped]
        public IFormFile? CourseLogoImage { get; set; }

        [NotMapped]
        public IFormFile? CourseContentFile { get; set; }
        // Navigation Property
        public List<Batch>? Batches { get; set; }

        public List<Video>? videos { get; set; }
    }
}

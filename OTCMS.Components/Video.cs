using OTCMS.Components;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OTCMS.Components
{
    public class Video
    {
        [Key]
        public int CourseVideoId { get; set; }

        [Required(ErrorMessage = "Emtpy title is not allowed")]
        [MaxLength(200, ErrorMessage = "Invalid Title")]
        [MinLength(10, ErrorMessage = "Invalid Title")]
        public string VideoTitle { get; set; } = string.Empty;

        [Required(ErrorMessage = "Emtpy Description is not allowed")]
        [MaxLength(2000, ErrorMessage = "Invalid Title")]
        [MinLength(10, ErrorMessage = "Invalid Title")]
        public string VideoDescription { get; set; } = string.Empty;

        [Required(ErrorMessage = "Emtpy Video link is not allowed")]
        [MaxLength(1500, ErrorMessage = "Invalid Title")]
        [MinLength(10, ErrorMessage = "Invalid Title")]
        public string VideoLink { get; set; } = string.Empty;

        public bool Status { get; set; }

        public DateTime CreatedOn { get; set; }

        [ForeignKey("course")]
        public int CourseId { get; set; }

        //Np
        public Course? course { get; set; }


    }
}
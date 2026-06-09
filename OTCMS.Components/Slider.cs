using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OTCMS.Components
{
    public class Slider
    {
        [Key]
        public int SliderId { get; set; }

        [Required(ErrorMessage = "Emtpy value is not allowed")]
        [MaxLength(30, ErrorMessage = "Invalid value")]
        [MinLength(3, ErrorMessage = "Invalid value")]
        public string SliderName { get; set; } = string.Empty;

        public bool SliderStatus { get; set; }

        [MaxLength(100, ErrorMessage = "Invalid value")]
        public string SliderText { get; set; } = string.Empty;

        [MaxLength(200, ErrorMessage = "Invalid value")]
        public string SliderDescriptionText { get; set; } = string.Empty;

        [Required(ErrorMessage = "Empty value is not allowed")]
        [Range(1, 10, ErrorMessage = "Invalid value")]
        public int SliderOrderNo { get; set; }
        public DateTime CreatedOn { get; set; }

        [MaxLength(300, ErrorMessage = "Invalid value")]
        public string SliderUrl { get; set; } = string.Empty;
        public string? SliderImagePath { get; set; } = string.Empty;

        // to handle image
        [NotMapped]
        public IFormFile? SliderImage { get; set; }
    }
}

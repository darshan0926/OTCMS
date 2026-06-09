using Microsoft.AspNetCore.Http;
using OTCMS.Components;

using System;

using System.ComponentModel.DataAnnotations;

using System.ComponentModel.DataAnnotations.Schema;

namespace OTCMS.Components

{

    public class BatchContent

    {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [RegularExpression("^[0-9]{1,6}$", ErrorMessage = "BCId must be numeric with up to 6 digits")]
        public int BCId { get; set; }

        [Required(ErrorMessage = "Empty value is not allowed")]
        [ForeignKey("Batch")]
        public int BatchId { get; set; }
       

        [Required(ErrorMessage = "Empty value is not allowed")]
        [MaxLength(200, ErrorMessage = "Invalid value")]
        [MinLength(3, ErrorMessage = "Invalid value")]
        public string Topic { get; set; } = string.Empty;

        [Required(ErrorMessage = "Empty value is not allowed")]
        public DateTime OnDate { get; set; }

        [MaxLength(500, ErrorMessage = "Invalid value")]
        public string? VideoFilePath { get; set; }

        [MaxLength(500, ErrorMessage = "Invalid value")]
        public string? NotesDocumentPath { get; set; }

        // Navigation Property

        public Batch? Batch { get; set; }

        [NotMapped]
        public IFormFile? VideoFile { get; set; }

        [NotMapped]
        public IFormFile? NotesDocument{ get; set; }

    }

}


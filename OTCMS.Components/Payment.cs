using System.ComponentModel.DataAnnotations;
using OTCMS.Components.ViewModels;
using System.ComponentModel.DataAnnotations.Schema;

namespace OTCMS.Components

{

    public class Payment

    {

        [Key]

        public int PaymentId { get; set; }

        // FK to User (only Students should create payments)

        [Required]

        [ForeignKey("User")]

        public int Id { get; set; }

        [Required]

        [ForeignKey("Batch")]

        public int BatchId { get; set; }

        [Required]

        [ForeignKey("Course")]

        public int CourseId { get; set; }

        [Required]

        [ForeignKey("PaymentType")]

        public int PaymentTypeId { get; set; }

        [Required]

        [DataType(DataType.DateTime)]

        public DateTime PaymentOn { get; set; } = DateTime.UtcNow;

        [Required]

        [Range(0, 999999.99, ErrorMessage = "PaymentAmount must be a valid decimal value")]

        [Column(TypeName = "decimal(18,2)")]

        public decimal PaymentAmount { get; set; }

        [Required]
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Inactive;

        // Navigation

        public User? User { get; set; }

        public Batch? Batch { get; set; }

        public Course? Course { get; set; }

        public PaymentType? PaymentType { get; set; }

        public PaymentCard? PaymentCard { get; set; }  // 1:1 (when card used)

    }

}


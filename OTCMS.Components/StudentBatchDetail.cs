using System.ComponentModel.DataAnnotations;

using System.ComponentModel.DataAnnotations.Schema;

namespace OTCMS.Components

{

    public class StudentBatchDetail

    {

        [Key]

        public int StudentBatchDetailsId { get; set; }  // identity PK

        // FK to User

        [Required]

        [ForeignKey("User")]

        public int UserId { get; set; }

        [Required]

        [ForeignKey("Batch")]

        public int BatchId { get; set; }

        [Required]

        [ForeignKey("Payment")]

        public int PaymentId { get; set; }

        [DataType(DataType.DateTime)]

        public DateTime EnrolledOn { get; set; } = DateTime.UtcNow;

        // Navigation

        public User? User { get; set; }

        public Batch? Batch { get; set; }

        public Payment? Payment { get; set; }

    }

}

using Newtonsoft.Json.Serialization;

using System.ComponentModel.DataAnnotations;

using System.ComponentModel.DataAnnotations.Schema;

namespace OTCMS.Components

{

    public class PaymentCard

    {

        [Key]

        public int PaymentCardId { get; set; }

        [Required]

        [ForeignKey("Payment")]

        public int PaymentId { get; set; }

        // Store only MASKED value (e.g., "**** **** **** 1234")

        [Required]

        [MaxLength(16, ErrorMessage = "Enter Valid Card Number")]

        [MinLength(16, ErrorMessage = "Enter Valid Card Number")]

        public string CardNumber { get; set; } = string.Empty;

        [Required]

        [MaxLength(100)]

        // Added \s to allow spaces and + to allow more than one letter

        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Only alphabets and spaces are allowed")]

        public string CardHolderName { get; set; } = string.Empty;


        // Store as "MM/YY"

        [Required]

        [RegularExpression(@"^(0[1-9]|1[0-2])\/\d{2}$", ErrorMessage = "Expiry must be in MM/YY format")]

        [MaxLength(5)]

        public string ExpiryDate { get; set; } = string.Empty;

        // DO NOT STORE CCV — for form binding only

        [Required]

        [MaxLength(4, ErrorMessage = "Enter Valid CVV Number")]

        [MinLength(3, ErrorMessage = "Enter Valid CVV Number")]

        public string? CCV { get; set; }

        [NotMapped]

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        // Navigation

        public Payment? Payment { get; set; }

    }

}
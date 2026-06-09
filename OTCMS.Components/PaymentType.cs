using System.ComponentModel.DataAnnotations;

using System.ComponentModel.DataAnnotations.Schema;

namespace OTCMS.Components

{

    public class PaymentType

    {

        [Key]

        [DatabaseGenerated(DatabaseGeneratedOption.None)]

        [RegularExpression("^[0-9]{1,4}$", ErrorMessage = "PaymentTypeId must be numeric with up to 4 digits")]

        public int PaymentTypeId { get; set; }

        [Required(ErrorMessage = "Empty value is not allowed")]

        [MaxLength(50, ErrorMessage = "Invalid value")]

        public string PaymentTypeName { get; set; } = string.Empty;

        // Navigation

        public List<Payment>? Payments { get; set; }

    }

}

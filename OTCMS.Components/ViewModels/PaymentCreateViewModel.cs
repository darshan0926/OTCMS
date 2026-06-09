using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace OTCMS.Components.ViewModels
{
    public class PaymentCreateViewModel
    {
        [Required] public int CourseId { get; set; }
        [Required] public int BatchId { get; set; }

        [Required, Range(0, 999999.99)]
        public decimal PaymentAmount { get; set; }

        [Required] public int PaymentTypeId { get; set; } // 1=Debit, 2=Credit


        // Card inputs
        [Required]
        [MaxLength(100)]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Only alphabets and spaces are allowed")]
        public string? CardHolderName { get; set; }

        [RegularExpression(@"^[0-9]{16}$", ErrorMessage = "Enter Valid Card Number")]
        public string? CardNumberPlain { get; set; }

        [RegularExpression(@"^(0[1-9]|1[0-2])\/\d{2}$")]
        public string? ExpiryMMYY { get; set; }

        [RegularExpression(@"^[0-9]{3,4}$", ErrorMessage = "Enter Valid CVV Number")]
        public string? CCV { get; set; }

        // UI helpers
        public SelectList? PaymentTypeOptions { get; set; }
        public string? CourseName { get; set; }
        public string? BatchName { get; set; }
        public decimal? BatchFee { get; set; }
    }
}
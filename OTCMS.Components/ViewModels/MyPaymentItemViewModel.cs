using OTCMS.Components;
using System;

namespace OTCMS.Components.ViewModels
{
    public class MyPaymentItemViewModel
    {
        public int PaymentId { get; set; }

        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;

        public int BatchId { get; set; }
        public string BatchName { get; set; } = string.Empty;

        public int PaymentTypeId { get; set; }
        public string PaymentTypeName { get; set; } = string.Empty;

        public decimal PaymentAmount { get; set; }
        public DateTime PaymentOn { get; set; }

        public PaymentStatus PaymentStatus { get; set; }

        // optional but useful
        public string? MaskedCardNumber { get; set; }
        public string? CardHolderName { get; set; }
    }
}
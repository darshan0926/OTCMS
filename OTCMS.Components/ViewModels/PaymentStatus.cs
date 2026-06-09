namespace OTCMS.Components.ViewModels

{

    public enum PaymentStatus

    {

        Inactive = 0,  // Created but not yet paid

        Active = 1,    // Paid by student (dummy success)

        Approved = 2,  // Approved by Admin

        Rejected = 3   // (Optional) Rejected by Admin

    }

}

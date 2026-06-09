using OTCMS.Components;

namespace OTCMS.Components.ViewModels
{
    public class BatchDetailsViewModel
    {
        // only for frontend

        public Course Course { get; set; } = new();
        public Batch Batch { get; set; } = new();
        public List<BatchContent> Contents { get; set; } = new();
    }
}

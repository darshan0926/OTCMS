namespace OTCMS.Components.ViewModels
{
    public class MyBatchItemViewModel
    {
        public int BatchId { get; set; }
        public string BatchName { get; set; } = string.Empty;

        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public string Timings { get; set; } = string.Empty;
        public string Duration { get; set; } = string.Empty;

        public decimal Fee { get; set; }

        public bool BatchStatus { get; set; } // active/inactive batch

        // Optional (nice to show)
        public int ContentCount { get; set; }
        public DateTime? NextClassDate { get; set; }
        public DateTime? LastClassDate { get; set; }
    }
}
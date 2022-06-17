namespace EmailPerformanceReport.Models
{
    internal class EmailBase
    {
        public int? emailMessageId { get; set; }
        public string? name { get; set; }
        public EmailStatistics? statistics { get; set; }
    }
}

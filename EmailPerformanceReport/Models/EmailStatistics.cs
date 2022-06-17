namespace EmailPerformanceReport.Models
{
    internal class EmailStatistics
    {
        public int recipients { get; set; }
        public int opens { get; set; }
        public int clicks { get; set; }
        public int unsubscribes { get; set; }
        public int bounces { get; set; }
    }
}

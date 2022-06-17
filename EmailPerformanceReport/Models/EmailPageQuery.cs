namespace EmailPerformanceReport.Models
{
    internal class EmailPageQuery : IPaginatedQuery<Email>
    {
        public EmailPageQuery()
        {
            items = new List<Email>();
        }

        public IEnumerable<Email> items { get; set; }
        public int count { get; set; }
        public string? nextPageLink { get; set; }
    }
}

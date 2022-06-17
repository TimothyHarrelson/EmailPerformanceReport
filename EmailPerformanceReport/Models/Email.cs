namespace EmailPerformanceReport.Models
{
    internal class Email : EmailBase
    {
        public Email()
        {
            variants = new List<EmailVariant>();
        }

        public IEnumerable<EmailVariant> variants { get; set; }
    }
}

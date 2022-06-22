using EmailPerformanceReport.Models;

namespace EmailPerformanceReport
{
    internal class Program
    {
        private static string _fileName = "EmailReport";

        static async Task Main(string[] args)
        {
            NgpRepository api;
            if (args.Length < 2)
            {
                Console.WriteLine($"Missing Parameters for API Username and Password");
                return;
            }
            else
            {
                api = new NgpRepository(args[0], args[1]);
            }

            if (args.Length > 2 && _fileName.IndexOfAny(System.IO.Path.GetInvalidFileNameChars()) != -1)
            {
                _fileName = args[2];
            }

            var emails = await api.GetAllEmailsAsync();
            emails = emails.OrderByDescending(email => email.emailMessageId);
            CreateEmailReportCSV(emails);

            Console.WriteLine($"Email report complete, file is {_fileName}.csv");
        }

        /// <summary>
        /// Creates an email report
        /// </summary>
        /// <param name="emails"></param>
        private static void CreateEmailReportCSV(IEnumerable<Email> emails)
        {
            using (var w = new StreamWriter($"./{_fileName}.csv"))
            {
                w.WriteLine("Email Message Id, Email Name, Recipients, Opens, Clicks, Unsubscribes, Bounces, Top Variant");
                w.Flush();

                foreach (var email in emails)
                {
                    var line = FormatEmailForCSV(email);
                    w.WriteLine(line);
                    w.Flush();
                }
            }
        }

        private static string FormatEmailForCSV(Email email)
        {
            string topVariant = GetTopVariantName(email);
            return $"{email.emailMessageId},{email.name},{email.statistics?.recipients},{email.statistics?.opens},{email.statistics?.clicks},{email.statistics?.unsubscribes},{email.statistics?.bounces},{topVariant}";
        }

        private static string GetTopVariantName(Email email)
        {
            if (email.variants == null)
            {
                return "";
            }

            //The requirements asked for the top variant to be the one with the highest percentage of opens, but the highest number of opens is the same thing for a given email
            return email.variants.OrderByDescending(variant => (variant.statistics == null) ? 0 : variant.statistics!.opens)
                .Select(variant => variant.name)
                .FirstOrDefault("")!;
        }
    }
}
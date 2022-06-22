using RestSharp;
using RestSharp.Authenticators;
using EmailPerformanceReport.Models;

namespace EmailPerformanceReport
{
    internal class NgpRepository
    {
        //See NGP 7 API documentation here:  https://docs.ngpvan.com/reference/getting-started-with-your-api-1

        readonly RestClient _client;
        readonly string _baseUrl = "https://api.myngp.com/v2";
        readonly string _username;
        readonly string _password;

        public NgpRepository (string username , string password)
        {
            _username = username;
            _password = password;

            var options = new RestClientOptions(_baseUrl);
            _client = new RestClient(options)
            {
                Authenticator = new HttpBasicAuthenticator(_username, _password),
            };
        }

        /// <summary>
        /// Asynchronously retrieves all emails from the NGP 7 API, including the statistics and variants for each email and the statistics of the variants. Result is not sorted.
        /// </summary>
        public async Task<IEnumerable<Email>> GetAllEmailsAsync()
        {

            /*
             * Loop through pages till the page comes back empty to get all records as currently it only grabs the top 25 (25 is max page size provided by the API currently)
             * I'd consider using nextPageLink but it doesnt seem to be returned with the data like the documentation says it should, so instead manage top and skip params manually
             * There is a danger with this methodology if there around a massive amount of records, we could run out of memory or just simply take a long time.
             * Consider adding a max page/record number
             */
            var emails = new List<Email>();
            var resEmails = new List<Email>();
            int pageCount = 0;
            int pageSize = 25;
            do
            {
                var request = new RestRequest("broadcastEmails", Method.Get);
                request.AddHeader("Accept", "application/json");
                request.AddQueryParameter("$top", pageSize, false);
                request.AddQueryParameter("$skip", pageCount, false);
                var response = await _client.ExecuteAsync<EmailPageQuery>(request);

                if (!response.IsSuccessful)
                {
                    Console.WriteLine($"ERROR: Failed to retrieve emails at page {pageCount}\nExcpetion: {response.ErrorException}");
                    break;
                }

                if (response.Data == null || response.Data.items.Count() <= 0)
                {
                    break;
                }

                resEmails.AddRange(response.Data.items);

                if (response.Data.items.Count() < pageSize)
                {
                    break;
                }

                pageCount += pageSize;
            } while (true);

            //This query does not return the statistics or variants, so each email will have to be requeried to get that info
            var emailTasks = new List<Task<Email?>>();
            foreach (var resEmail in resEmails)
            {
                emailTasks.Add(GetEmailDetailsAsync(resEmail.emailMessageId));   
            }

            var emailDetails = await Task.WhenAll(emailTasks);

            foreach(var email in emailDetails)
            {
                if (email != null)
                {
                    emails.Add(email);
                }
            }

            return emails;
        }

        /// <summary>
        /// Retrieves a single email from the NGP 7 API, including the statistics of the email, its variants, and the statistics of the variants.
        /// </summary>
        /// <param name="emailMessageId">The email message id of the email to retrieve</param>
        public async Task<Email?> GetEmailDetailsAsync(int? emailMessageId)
        {
            if (!emailMessageId.HasValue) 
            {
                return null;
            }

            var request = new RestRequest($"broadcastEmails/{emailMessageId}", Method.Get);
            request.AddHeader("Accept", "application/json");
            request.AddQueryParameter("$expand", "statistics", false);
            var response = await _client.ExecuteAsync<Email>(request);
            
            if (!response.IsSuccessful)
            {
                Console.WriteLine($"ERROR: Failed to retrieve email details for email message id {emailMessageId}\nExcpetion: {response.ErrorException}");
                return null;
            }

            if (response?.Data != null)
            {
                var email = response!.Data;

                //This query does not return the statistics of the variants, so each email will have to be requeried to get that info
                var variants = new List<EmailVariant>(email.variants.Count());
                var variantDetailTasks = new List<Task<EmailVariant?>>(email.variants.Count());
                foreach (var resVariant in email.variants)
                {
                    variantDetailTasks.Add(GetVariantDetailsAsync(emailMessageId, resVariant.emailMessageVariantId));
                }

                var variantDetails = await Task.WhenAll(variantDetailTasks);

                foreach (var variant in variantDetails)
                {
                    if (variant != null)
                    {
                        variants.Add(variant);
                    }
                }
                email.variants = variants;
                return email;
            }

            return null;
        }

        /// <summary>
        /// Retrieves a single variant of an email, including the statistics of the variant.
        /// </summary>
        /// <param name="emailMessageId">The email message id of the email the variant is a variant of</param>
        /// <param name="emailMessageVariantId">The email message variant id of the variant</param>
        public async Task<EmailVariant?> GetVariantDetailsAsync(int? emailMessageId, int? emailMessageVariantId)
        {
            if (!emailMessageId.HasValue || !emailMessageVariantId.HasValue)
            {
                return null;
            }

            var request = new RestRequest($"broadcastEmails/{emailMessageId}/variants/{emailMessageVariantId}", Method.Get);
            request.AddHeader("Accept", "application/json");
            request.AddQueryParameter("$expand", "statistics", false);
            var response = await _client.ExecuteAsync<EmailVariant>(request);

            if (!response.IsSuccessful)
            {
                Console.WriteLine($"ERROR: Failed to retrieve email variant details for email message id {emailMessageId} variant id {emailMessageVariantId}\nExcpetion: {response.ErrorException}");
                return null;
            }

            if (response?.Data != null)
            {
                var variant = response!.Data;

                return variant;
            }

            return null;
        }
    }
}

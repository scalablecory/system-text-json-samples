using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace json_test
{
    partial class Program
    {
        static async Task Main(string[] args)
        {
            using CancellationTokenSource cts = new CancellationTokenSource();
            Console.CancelKeyPress += (s, e) => cts.Cancel();

            List<Complaint> complaints;
            using (HttpClient client = new HttpClient())
            using (HttpResponseMessage response = await client.GetAsync("http://data.consumerfinance.gov/api/views.json", HttpCompletionOption.ResponseHeadersRead, cts.Token).ConfigureAwait(false))
            using (Stream responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
            {
                complaints = await Complaint.ReadAsync(responseStream, cts.Token);
            }

            Console.WriteLine($"Read {complaints.Count:N0} complaints.");
        }
    }
}

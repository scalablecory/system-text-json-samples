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
        const string SampleUri = "http://data.consumerfinance.gov/api/views.json";

        static async Task Main(string[] args)
        {
            using CancellationTokenSource cts = new CancellationTokenSource();
            Console.CancelKeyPress += (s, e) => cts.Cancel();

            await ReadViaStream(cts.Token);
        }

        static async Task ReadViaSpan(CancellationToken cancellationToken)
        {
            byte[] data;

            using (HttpClient client = new HttpClient())
            using (HttpResponseMessage response = await client.GetAsync(SampleUri, cancellationToken).ConfigureAwait(false))
            {
                data = await response.Content.ReadAsByteArrayAsync();
            }

            List<Complaint> complaints = Complaint.Read(data);
            Console.WriteLine($"Read {complaints.Count:N0} complaints.");
        }

        static async Task ReadViaStream(CancellationToken cancellationToken)
        {
            List<Complaint> complaints;
            using (HttpClient client = new HttpClient())
            using (HttpResponseMessage response = await client.GetAsync(SampleUri, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false))
            using (Stream responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
            {
                complaints = await Complaint.ReadAsync(responseStream, cancellationToken);
            }

            Console.WriteLine($"Read {complaints.Count:N0} complaints.");
        }
    }
}

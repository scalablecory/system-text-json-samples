using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace json_test
{
    sealed class Complaint
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int ViewCount { get; set; }

        public static async Task<List<Complaint>> ReadAsync(Stream stream, CancellationToken cancellationToken)
        {
            //return await JsonParser.ParseSimpleAsync<List<Complaint>, ComplaintsParser>(stream, cancellationToken).ConfigureAwait(false);
            return await JsonParser.ParseNoCopyAsync<List<Complaint>, ComplaintsParser>(stream, cancellationToken).ConfigureAwait(false);
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
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
            return await JsonParser.ParseSimpleAsync<List<Complaint>, ComplaintsParser>(stream, cancellationToken).ConfigureAwait(false);
        }

        public static List<Complaint> Read(ReadOnlySpan<byte> span)
        {
            var complaints = new List<Complaint>();

            var reader = new Utf8JsonReader(span, true, default);

            if (!reader.Read())
            {
                return complaints;
            }

            if (reader.TokenType != JsonTokenType.StartArray)
            {
                throw new Exception("Unexpected token found; expected array.");
            }

            while (ReadComplaint(ref reader, out Complaint complaint))
            {
                complaints.Add(complaint);
            }

            return complaints;
        }

        static bool ReadComplaint(ref Utf8JsonReader reader, out Complaint complaint)
        {
            if (!reader.Read())
            {
                throw new Exception("Unexpected end of stream; expected end of array or a complaint object.");
            }

            if (reader.TokenType == JsonTokenType.EndArray)
            {
                complaint = default;
                return false;
            }

            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new Exception("Unexpected token found; expected end of array or a complaint object.");
            }

            complaint = new Complaint();

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return true;
                }

                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    throw new Exception("Unexpected token found; expected property or end of object.");
                }

                string propertyName = reader.GetString();

                if (!reader.Read())
                {
                    throw new Exception("Unexpected end of stream; expected property value.");
                }

                switch (propertyName)
                {
                    case "id":
                        complaint.Id = reader.GetString();
                        break;
                    case "name":
                        complaint.Name = reader.GetString();
                        break;
                    case "viewCount":
                        complaint.ViewCount = reader.GetInt32();
                        break;
                    default:
                        if (!reader.TrySkip())
                        {
                            throw new Exception("Unexpected end of stream; expected property value.");
                        }
                        break;
                }
            }

            throw new Exception("Unexpected end of stream; expected property or end of object.");
        }
    }
}

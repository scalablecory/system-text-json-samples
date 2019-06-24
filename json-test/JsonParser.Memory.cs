using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace json_test
{
    static partial class JsonParser
    {
        public static T Parse<T, TParser>(ReadOnlySpan<byte> span)
            where TParser : IJsonParser<T>, new()
        {
            var reader = new Utf8JsonReader(span, true, new JsonReaderState());
            var parser = new TParser();

            if (!parser.TryContinueParse(ref reader))
            {
                throw new Exception("unexpected end of document.");
            }

            return parser.FinalValue;
        }
    }
}

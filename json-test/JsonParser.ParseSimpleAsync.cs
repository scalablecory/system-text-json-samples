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
        public static async Task<T> ParseSimpleAsync<T, TParser>(Stream stream, CancellationToken cancellationToken)
            where TParser : IJsonParser<T>, new()
        {
            var buffer = new byte[4096];
            int fill = 0, consumed = 0;
            bool done = false;

            var readerState = new JsonReaderState();
            var parser = new TParser();

            while(true)
            {
                if (!done)
                {
                    if (fill == buffer.Length)
                    {
                        if (consumed != 0)
                        {
                            buffer.AsSpan(consumed).CopyTo(buffer);
                            fill -= consumed;
                            consumed = 0;
                        }
                        else
                        {
                            Array.Resize(ref buffer, buffer.Length * 3 / 2);
                        }
                    }

                    int read = await stream.ReadAsync(buffer.AsMemory(fill), cancellationToken).ConfigureAwait(false);

                    fill += read;
                    done = read == 0;
                }

                if (!DoReadSync())
                {
                    if (done) throw new Exception("unexpected end of document.");
                }
                else
                {
                    return parser.FinalValue;
                }
            }

            bool DoReadSync()
            {
                var reader = new Utf8JsonReader(buffer.AsSpan(consumed, fill - consumed), done, readerState);
                bool res = parser.TryContinueParse(ref reader);

                consumed += (int)reader.CurrentState.BytesConsumed;
                readerState = reader.CurrentState;

                return res;
            }
        }
    }
}

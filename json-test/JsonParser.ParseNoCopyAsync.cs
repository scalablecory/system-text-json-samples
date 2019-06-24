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
        public static async Task<T> ParseNoCopyAsync<T, TParser>(Stream stream, CancellationToken cancellationToken)
            where TParser : IJsonParser<T>, new()
        {
            ArrayPool<byte> pool = ArrayPool<byte>.Shared;
            int rentSize = 4096;

            MyBuffer firstBuffer, lastBuffer;
            int fill = 0, consumed = 0;
            bool done = false;

            firstBuffer = lastBuffer = new MyBuffer(pool.Rent(rentSize), 0);

            var readerState = new JsonReaderState();
            var parser = new TParser();

            while(true)
            {
                if (!done)
                {
                    if (fill == lastBuffer.Memory.Length)
                    {
                        rentSize = Math.Min(65536, rentSize * 3 / 2);
                        var newLastBuffer = new MyBuffer(pool.Rent(rentSize), lastBuffer.RunningIndex + lastBuffer.Memory.Length);

                        lastBuffer.SetNext(newLastBuffer);
                        lastBuffer = newLastBuffer;
                        fill = 0;
                    }

                    int read = await stream.ReadAsync(lastBuffer.WritableMemory.AsMemory(fill), cancellationToken).ConfigureAwait(false);

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
                var availableSequence = new ReadOnlySequence<byte>(firstBuffer, consumed, lastBuffer, fill);

                var reader = new Utf8JsonReader(availableSequence, done, readerState);
                bool res = parser.TryContinueParse(ref reader);

                long newConsumed = reader.BytesConsumed;

                while (newConsumed != 0)
                {
                    int left = (firstBuffer == lastBuffer ? fill : firstBuffer.Memory.Length) - consumed;
                    int take = (int)Math.Min(left, newConsumed);

                    consumed += take;
                    newConsumed -= take;

                    if (consumed == firstBuffer.Memory.Length)
                    {
                        consumed = 0;

                        if (firstBuffer != lastBuffer)
                        {
                            pool.Return(firstBuffer.WritableMemory);
                            firstBuffer = (MyBuffer)firstBuffer.Next;
                        }
                        else
                        {
                            fill = 0;
                        }
                    }
                }

                readerState = reader.CurrentState;

                return res;
            }
        }

        sealed class MyBuffer : ReadOnlySequenceSegment<byte>
        {
            public byte[] WritableMemory { get; }

            public MyBuffer(byte[] memory, long runningIndex)
            {
                Memory = memory;
                RunningIndex = runningIndex;
                WritableMemory = memory;
            }

            public void SetNext(MyBuffer nextBuffer)
            {
                Next = nextBuffer;
            }
        }
    }
}

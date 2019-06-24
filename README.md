# System.Text.Json Samples

This looks at how one might write a parser on top of System.Text.Json

See ComplaintParser for a resumable, I/O-agnostic parser that uses the lower-level `Utf8JsonReader`.

See JsonParser.Memory.cs for a simple Span-based implementation that uses that parser.

See JsonParser.ParseSimpleAsync.cs for a Stream-based implementation that uses a trivial growing buffer.

See Jsonparser.ParseNoCopyAsync.cs for a Stream-based implementation that uses Sequences to avoid copying when growing buffers.

Sample data used: http://data.consumerfinance.gov/api/views.json
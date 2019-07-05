# System.Text.Json Samples

System.Text.Json is a high-performance JSON API added in .NET Core 3.

This looks at how one might use its various features.

Sample data used: http://data.consumerfinance.gov/api/views.json

## JsonSerializer

TODO.

## JsonDocument

TODO.

## Utf8JsonReader

`Utf8JsonReader` is a lower-level pull parser similar to `XmlReader`.

### Parsing synchronously

`Complaint.Read` implements a synchronous parser on top of `ReadOnlySpan`.

This is the simplest parser you can write on top of `Utf8JsonReader`, with each "object" being parsed in its own method and a clear call stack as you go through the document.

### Parsing asynchronously

`Complaint.ReadAsync` implements an asynchronous parser on top of `Stream`.

This does not load the entire `Stream` into memory: instead, it is parsed in reasonably sized chunks. We must handle the case where `Utf8JsonReader` exhausted the current buffer, and we must read the next chunk. Because `Utf8JsonReader` is a `ref struct`, we can not use it in an `async` method and must instead implement our own state machine manually. This makes things a bit harder to follow.

This is implemented via a resumable, I/O-agnostic parser `ComplaintParser` that is passed to one of three methods which handle the I/O:
* See JsonParser.Memory.cs for a simple Span-based implementation that uses that parser.
* See JsonParser.ParseSimpleAsync.cs for a Stream-based implementation that implements trivial copy & grow buffering.
* See Jsonparser.ParseNoCopyAsync.cs for a Stream-based implementation that implements sequences to avoid copying when growing buffers.
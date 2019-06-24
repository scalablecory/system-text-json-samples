using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace json_test
{
    interface IJsonParser<T>
    {
        T FinalValue { get; }
        bool TryContinueParse(ref Utf8JsonReader reader);
    }
}

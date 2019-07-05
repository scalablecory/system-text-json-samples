using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Buffers;

namespace json_test
{
    struct ComplaintsParser : IJsonParser<List<Complaint>>
    {
        State state;
        List<Complaint> complaints;
        Complaint currentComplaint;

        public List<Complaint> FinalValue => complaints;

        public bool TryContinueParse(ref Utf8JsonReader reader)
        {
            switch (state)
            {
                case State.Start:
                    if (!reader.Read()) return false;
                    if (reader.TokenType != JsonTokenType.StartArray) throw new Exception("Unexpected token found; expected array.");

                    complaints = new List<Complaint>();
                    state = State.ComplaintList;
                    goto case State.ComplaintList;
                case State.ComplaintList:
                ComplaintList:
                    if (!reader.Read()) return false;

                    switch (reader.TokenType)
                    {
                        case JsonTokenType.StartObject:
                            currentComplaint = new Complaint();
                            complaints.Add(currentComplaint);
                            state = State.Complaint;
                            goto Complaint;
                        case JsonTokenType.EndArray:
                            state = State.Done;
                            return true;
                        default:
                            throw new Exception("Unexpected token found; expected object or end of array.");
                    }
                case State.Complaint:
                Complaint:
                    // For simplicity, this section reads property names and values together.
                    // This is a trade-off: if a value fails to fully read, the property name
                    // will be stuck in the buffer and when we next resume it will need to be
                    // re-parsed. This is probably okay for most cases when values are expected
                    // to be small, but for larger values the value parsing should be broken out
                    // into its own step in the state machine.

                    Utf8JsonReader oldReader = reader;

                    if (!reader.Read()) return false;

                    switch (reader.TokenType)
                    {
                        case JsonTokenType.PropertyName:
                            string propertyName = reader.GetString();

                            if (!reader.Read())
                            {
                                reader = oldReader;
                                return false;
                            }

                            switch (propertyName)
                            {
                                case "id":
                                    currentComplaint.Id = reader.GetString();
                                    goto Complaint;
                                case "name":
                                    currentComplaint.Name = reader.GetString();
                                    goto Complaint;
                                case "viewCount":
                                    currentComplaint.ViewCount = reader.GetInt32();
                                    goto Complaint;
                                default:
                                    if (!reader.TrySkip())
                                    {
                                        reader = oldReader;
                                        return false;
                                    }
                                    goto Complaint;
                            }
                        case JsonTokenType.EndObject:
                            state = State.ComplaintList;
                            goto ComplaintList;
                        default:
                            throw new Exception("Unexpected token found; expected key or end of object.");
                    }
                case State.Done:
                    return true;
                default:
                    throw new Exception("Unknown state value; this should never be hit.");
            }
        }

        enum State
        {
            Start,
            ComplaintList,
            Complaint,
            Done
        }
    }
}

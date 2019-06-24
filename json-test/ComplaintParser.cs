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
                    Utf8JsonReader oldReader = reader;

                    if (!reader.Read()) return false;

                    switch (reader.TokenType)
                    {
                        case JsonTokenType.PropertyName:
                            switch (reader.GetString())
                            {
                                case "id":
                                    if (!reader.Read())
                                    {
                                        reader = oldReader;
                                        return false;
                                    }
                                    currentComplaint.Id = reader.GetString();
                                    goto Complaint;
                                case "name":
                                    if (!reader.Read())
                                    {
                                        reader = oldReader;
                                        return false;
                                    }
                                    currentComplaint.Name = reader.GetString();
                                    goto Complaint;
                                case "viewCount":
                                    if (!reader.Read())
                                    {
                                        reader = oldReader;
                                        return false;
                                    }
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

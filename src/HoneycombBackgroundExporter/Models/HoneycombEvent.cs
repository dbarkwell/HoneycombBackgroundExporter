#nullable enable
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace HoneycombBackgroundExporter.Models
{
    internal class HoneycombEvent
    {
        public HoneycombEvent(string serviceName, ActivityTraceId traceId)
        {
            Data = new Dictionary<string, object?>
            {
                {"service_name", serviceName}, {"trace.trace_id", traceId.ToString()}
            };
            // add library https://github.com/open-telemetry/opentelemetry-specification/pull/494/files
        }

        [JsonPropertyName("data")] 
        public Dictionary<string, object?> Data { get; }

        public void PopulateData(IEnumerable<KeyValuePair<string, string?>> data)
        {
            foreach (var (key, value) in data)
            {
                Data.Add(key, value);
            }
        }

        public void PopulateData(IEnumerable<KeyValuePair<string, object>>? data)
        {
            if (data == null)
                return;

            foreach (var (key, value) in data)
            {
                Data.Add(key, value);
            }
        }
    }
}
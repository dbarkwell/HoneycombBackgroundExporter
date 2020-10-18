using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using HoneycombBackgroundExporter.Models;

namespace HoneycombBackgroundExporter.Service
{
    internal class HoneycombService : IHoneycombService
    {
        private const string BaseUri = "https://api.honeycomb.io/1/";
        private const string EventsUri = BaseUri + "events/";
        private const string BatchUri = BaseUri + "batch/";
        private readonly HttpClient _httpClient;
        
        public HoneycombService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<HttpResponseMessage> SendAsync(string dataset, DateTime eventTime,
            HoneycombEvent honeycombEvent)
        {
            return await SendHoneycombRequestAsync($"{EventsUri}/{dataset}", eventTime,
                    honeycombEvent)
                .ConfigureAwait(false);
        }

        public async Task<HttpResponseMessage> SendBatchAsync(string dataset, DateTime eventTime,
            IEnumerable<HoneycombEvent> honeycombEvents)
        {
            return await SendHoneycombRequestAsync($"{BatchUri}/{dataset}", eventTime,
                    honeycombEvents)
                .ConfigureAwait(false);
        }

        private async Task<HttpResponseMessage> SendHoneycombRequestAsync<T>(string uri, DateTime eventTime, T content)
        {
            _httpClient.DefaultRequestHeaders.Add("X-Honeycomb-Event-Time",
                eventTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));
            return await _httpClient.PostAsync(
                uri,
                new StringContent(JsonSerializer.Serialize(content))).ConfigureAwait(false);
        }
    }
}
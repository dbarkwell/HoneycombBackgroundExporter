using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using HoneycombBackgroundExporter.Models;

namespace HoneycombBackgroundExporter.Service
{
    internal interface IHoneycombService
    {
        Task<HttpResponseMessage> SendAsync(string dataset, DateTime eventTime, HoneycombEvent honeycombEvent);

        Task<HttpResponseMessage> SendBatchAsync(string dataset, DateTime eventTime,
            IEnumerable<HoneycombEvent> honeycombEvents);
    }
}
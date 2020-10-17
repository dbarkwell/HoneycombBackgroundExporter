using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using HoneycombBackgroundExporter.Models;
using HoneycombBackgroundExporter.Service;
using Microsoft.Extensions.Hosting;

namespace HoneycombBackgroundExporter
{
    internal class HoneycombBackgroundService : BackgroundService
    {
        private readonly ChannelReader<Activity> _channelReader;
        private readonly string _dataset;
        private readonly IHoneycombService _honeycombService;

        public HoneycombBackgroundService(string dataset, ChannelReader<Activity> channelReader,
            IHoneycombService honeyCombService)
        {
            _dataset = dataset;
            _channelReader = channelReader;
            _honeycombService = honeyCombService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await foreach (var activity in _channelReader.ReadAllAsync(stoppingToken))
            {
                if (activity.Tags.Any(tag =>
                    tag.Key == "http.url" &&
                    tag.Value != null &&
                    tag.Value.StartsWith("https://api.honeycomb.io/")))
                    continue;

                var list = new List<HoneycombEvent>();
                var ev = new HoneycombEvent(activity.DisplayName, activity.TraceId);

                if (activity.ParentId != null)
                    ev.Data.Add("trace.parent_id", activity.ParentId);

                ev.Data.Add("trace.span_id", activity.SpanId.ToString());
                ev.Data.Add("duration_ms", activity.Duration.Milliseconds);

                ev.PopulateData(activity.Tags);
                list.Add(ev);

                foreach (var evt in activity.Events)
                {
                    var messageEvent = new HoneycombEvent(activity.DisplayName, activity.TraceId);
                    messageEvent.PopulateData(evt.Tags);

                    messageEvent.Data.Add("meta.annotation_type", "span_event");
                    messageEvent.Data.Add("trace.parent_id", activity.SpanId.ToString());
                    messageEvent.Data.Add("name", evt.Name);
                    list.Add(messageEvent);
                }

                foreach (var link in activity.Links)
                {
                    var linkEvent = new HoneycombEvent(activity.DisplayName, activity.TraceId);
                    linkEvent.PopulateData(link.Tags);
                    linkEvent.Data.Add("meta.annotation_type", "link");
                    linkEvent.Data.Add("trace.link.span_id", link.Context.SpanId.ToString());
                    linkEvent.Data.Add("trace.link.trace_id", link.Context.TraceId.ToString());
                    list.Add(linkEvent);
                }

                await _honeycombService.SendBatchAsync(_dataset, activity.StartTimeUtc, list).ConfigureAwait(false);
            }
        }
    }
}
using System;
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
        private readonly Func<Activity, string> _dataset;
        private readonly IHoneycombService _honeycombService;

        public HoneycombBackgroundService(ChannelReader<Activity> channelReader, string dataset, 
            IHoneycombService honeyCombService)
        {
            _channelReader = channelReader;
            _dataset = activity => dataset;
            _honeycombService = honeyCombService;
        }
        
        public HoneycombBackgroundService(ChannelReader<Activity> channelReader, Func<Activity, string> dataset, 
            IHoneycombService honeyCombService)
        {
            _channelReader = channelReader;
            _dataset = dataset;
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
                
                var dataset = _dataset.Invoke(activity);
                if (string.IsNullOrWhiteSpace(dataset))
                    throw new ApplicationException("Dataset could not be found."); 
                        
                var list = new List<HoneycombEvent>();
                var ev = new HoneycombEvent(activity.DisplayName, activity.TraceId);

                if (activity.ParentId != null)
                    ev.Data.Add("trace.parent_id", activity.ParentId);

                ev.Data.Add("trace.span_id", activity.SpanId.ToString());
                ev.Data.Add("duration_ms", activity.Duration.Milliseconds);

                ev.PopulateData(activity.Tags);
                list.Add(ev);
                
                list.AddRange(GetActivityEvents(activity.Events, activity.DisplayName, activity.TraceId, activity.SpanId));
                list.AddRange(GetActivityLinks(activity.Links, activity.DisplayName, activity.TraceId, activity.SpanId));

                await _honeycombService.SendBatchAsync(dataset, activity.StartTimeUtc, list).ConfigureAwait(false);
            }
        }

        private static IEnumerable<HoneycombEvent> GetActivityEvents(
            IEnumerable<ActivityEvent> activityEvents, 
            string displayName, 
            ActivityTraceId traceId, 
            ActivitySpanId spanId)
        {
            foreach (var evt in activityEvents)
            {
                var messageEvent = new HoneycombEvent(displayName, traceId);
                messageEvent.PopulateData(evt.Tags);

                messageEvent.Data.Add("meta.annotation_type", "span_event");
                messageEvent.Data.Add("trace.parent_id", spanId.ToString());
                messageEvent.Data.Add("name", evt.Name);
                
                yield return messageEvent;
            }
        }

        private static IEnumerable<HoneycombEvent> GetActivityLinks(
            IEnumerable<ActivityLink> activityLinks,
            string displayName,
            ActivityTraceId traceId,
            ActivitySpanId spanId)
        {
            foreach (var link in activityLinks)
            {
                var linkEvent = new HoneycombEvent(displayName, traceId);
                linkEvent.PopulateData(link.Tags);
                linkEvent.Data.Add("meta.annotation_type", "link");
                linkEvent.Data.Add("trace.link.span_id", link.Context.SpanId.ToString());
                linkEvent.Data.Add("trace.link.trace_id", link.Context.TraceId.ToString());
                
                yield return linkEvent;
            }
        }
    }
}
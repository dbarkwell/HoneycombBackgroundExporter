using System.Diagnostics;
using System.Threading.Channels;
using HoneycombBackgroundExporter.Service;
using Microsoft.Extensions.DependencyInjection;

namespace HoneycombBackgroundExporter.Middleware
{
    public static class HoneycombExporterMiddlewareExtensions
    {
        public static void AddHoneycombExporter(this IServiceCollection services, string teamId, string dataset,
            ChannelReader<Activity> channelReader)
        {
            services.AddHttpClient<HoneycombService>(httpClient =>
            {
                httpClient.DefaultRequestHeaders.Add("X-Honeycomb-Team", teamId);
            });
            services.AddHostedService(
                sp =>
                    new HoneycombBackgroundService(dataset, channelReader, sp.GetService<HoneycombService>()));
        }
    }
}
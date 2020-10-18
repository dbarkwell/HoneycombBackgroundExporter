using System;
using System.Diagnostics;
using System.Threading.Channels;
using HoneycombBackgroundExporter.Service;
using Microsoft.Extensions.DependencyInjection;

namespace HoneycombBackgroundExporter.Middleware
{
    public static class HoneycombExporterMiddlewareExtensions
    {
        public static void AddHoneycombExporter(
            this IServiceCollection services, 
            ChannelReader<Activity> channelReader, 
            string teamId, 
            string dataset)
        {
            services.AddHttpClient<HoneycombService>(httpClient =>
            {
                httpClient.DefaultRequestHeaders.Add("X-Honeycomb-Team", teamId);
            });
            services.AddHostedService(
                sp =>
                    new HoneycombBackgroundService(channelReader, dataset, sp.GetService<HoneycombService>()));
        }
        
        public static void AddHoneycombExporter(
            this IServiceCollection services, 
            ChannelReader<Activity> channelReader, 
            string teamId, 
            Func<Activity, string> dataset)
        {
            services.AddHttpClient<HoneycombService>(httpClient =>
            {
                httpClient.DefaultRequestHeaders.Add("X-Honeycomb-Team", teamId);
            });
            services.AddHostedService(
                sp =>
                    new HoneycombBackgroundService(channelReader, dataset, sp.GetService<HoneycombService>()));
        }
    }
}
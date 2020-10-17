using System.Collections.Generic;
using System.Diagnostics;
using HoneycombBackgroundExporter.Models;
using Xunit;

namespace HoneycombBackgroundExporterTests.Models
{
    public class HoneycombEventTests
    {
        public class PopulateData
        {
            [Fact]
            public void WhenDataHasKeyAndStringValue_ShouldAddToDataDictionary()
            {
                var honeyCombEvent = new HoneycombEvent("serviceName", new ActivityTraceId());
                var list = new List<KeyValuePair<string, string>> {new KeyValuePair<string, string>("hello", "world")};
                
                honeyCombEvent.PopulateData(list);
                
                Assert.True(honeyCombEvent.Data.ContainsKey("hello"));
                Assert.Equal("world", honeyCombEvent.Data["hello"].ToString());
            }
            
            [Fact]
            public void WhenDataHasKeyAndNullValue_ShouldAddToDataDictionary()
            {
                var honeyCombEvent = new HoneycombEvent("serviceName", new ActivityTraceId());
                var list = new List<KeyValuePair<string, string>> {new KeyValuePair<string, string>("hello", null)};
                
                honeyCombEvent.PopulateData(list);
                
                Assert.True(honeyCombEvent.Data.ContainsKey("hello"));
                Assert.Null(honeyCombEvent.Data["hello"]);
            }
            
            [Fact]
            public void WhenDataHasKeyAndObjectValue_ShouldAddToDataDictionary()
            {
                var honeyCombEvent = new HoneycombEvent("serviceName", new ActivityTraceId());
                var list = new List<KeyValuePair<string, object>> {new KeyValuePair<string, object>("hello", 1)};
                
                honeyCombEvent.PopulateData(list);
                
                Assert.True(honeyCombEvent.Data.ContainsKey("hello"));
                Assert.Equal(1, honeyCombEvent.Data["hello"]);
            }
        }
    }
}
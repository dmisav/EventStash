using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using GrokNet;
using Pipeline.PipelineCore.StepsCore;

namespace Pipeline.Steps
{
    public class EnhancedParserStep : RegularStep<string, string>
    {
        private readonly EnhancedFirewallPaloAltoParser _parser = new();

        public override string ProcessItem(string logLine)
        {
            return _parser.Parse(logLine);
        }
    }

    public class EnhancedFirewallPaloAltoParser
    {
        private const string MainGrokStr =
            "^([^,]+),(?<tempLocalEventTime>[^,]+),%{NUMBER},TRAFFIC,(start|end|drop|deny),[^,]*,[^,]*,%{IP:InternalIP},%{IP:DestinationIP},(%{IP:NATSourceAddress})?,(%{IP:NATDestinationAddress})?,(?<PolicyName>[^,]+),(?<OperationBy>[^,]*),[^,]*,(?<App>[^,]+),[^,]*,(?<SourceZone>[^,]+),(?<DestinationZone>[^,]+),[^,]*,[^,]*,[^,]*,[^,]*,%{NUMBER:SessionID},%{NUMBER}?,%{NUMBER:SourcePort},%{NUMBER:DestinationPort},%{NUMBER:NATSourcePort},%{NUMBER:NATDestinationPort},%{BASE16NUM},(?<Protocol>(udp|tcp|icmp)),(?<VendorStatusReason>(allow|deny|drop.ICMP|drop|reset.both|reset.client|reset.server)),%{NUMBER:Bytes},%{NUMBER:UploadSize},%{NUMBER:DownloadSize},%{NUMBER},[^,]*,[^,]*,(?<ContentFilteringCategory>[^,]+)%{GREEDYDATA}($|\r)";

        private const string StreamVendor = "FirewallPaloAlto";

        private readonly Grok _mainGrok = new(MainGrokStr);
        private readonly DummyDateParser _dateParser = new();

        private static readonly HashSet<string> BlackListedFields = new()
        {
            "NATSourceAddress",
            "NATDestinationAddress"
        };

        public string Parse(string logLine)
        {
            var grokResult = _mainGrok.Parse(logLine);

            if (!grokResult.Any())
            {
                return null;
            }


            var options = new JsonWriterOptions
            {
                Indented = false
            };
            using var stream = new MemoryStream();
            using var writer = new Utf8JsonWriter(stream, options);

            writer.WriteStartObject();
            writer.WriteString("EventType", "NetworkSession");
            writer.WriteString("OperationSource", "Log");
            writer.WriteString("EventOperation", "Accessed");
            writer.WriteString("ObjectType", "Firewall");
            writer.WriteString("StreamType", "Firewall");
            writer.WriteString("type", StreamVendor);

            foreach (var grokItem in grokResult)
            {
                if (BlackListedFields.Contains(grokItem.Key))
                    continue;

                if (grokItem.Key == "tempLocalEventTime")
                {
                    var (localEventTime, eventTime) = _dateParser.ParseDate((string) grokItem.Value);
                    writer.WriteNumber("LocalEventTime", localEventTime);
                    writer.WriteNumber("EventTime", eventTime);
                }
                else
                {
                    writer.WriteString(grokItem.Key, grokItem.Value.ToString());
                }
            }

            writer.WriteEndObject();
            writer.Flush();

            return Encoding.UTF8.GetString(stream.ToArray());
        }
    }
}
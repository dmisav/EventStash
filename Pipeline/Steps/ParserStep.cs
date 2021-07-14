using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using GrokNet;
using Pipeline.PipelineCore.StepsCore;

namespace Pipeline.Steps
{
    public class ParserStep : RegularStep<string, Dictionary<string, string>>
    {
        private readonly FirewallPaloAltoParser _parser = new();

        public override Dictionary<string, string> ProcessItem(string item)
        {
            return _parser.Parse(item);
        }
    }

    public class FirewallPaloAltoParser
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

        public Dictionary<string, string> Parse(string logLine)
        {
            var grokResult = _mainGrok.Parse(logLine);
            var result = new Dictionary<string, string>();

            if (!grokResult.Any())
            {
                return null;
            }

            result.Add("EventType", "NetworkSession");
            result.Add("OperationSource", "Log");
            result.Add("EventOperation", "Accessed");
            result.Add("ObjectType", "Firewall");
            result.Add("StreamType", "Firewall");
            result.Add("type", StreamVendor);

            foreach (var grokItem in grokResult)
            {
                if (BlackListedFields.Contains(grokItem.Key))
                    continue;

                if (grokItem.Key == "tempLocalEventTime")
                {
                    var (localEventTime, eventTime) = _dateParser.ParseDate((string) grokItem.Value);
                    result.Add("LocalEventTime", localEventTime.ToString());
                    result.Add("EventTime", eventTime.ToString());
                }
                else
                {
                    result.Add(grokItem.Key, grokItem.Value.ToString());
                }
            }

            return result;
        }
    }

    public class DummyDateParser
    {
        private const string SupportedFormat = "yyyy/MM/dd HH:mm:ss";
        private readonly DateTime _unixStartDate = new(1970, 1, 1);

        public (double localEventTime, double eventTime) ParseDate(string dt)
        {
            var datetime = DateTime.ParseExact(dt, SupportedFormat, CultureInfo.InvariantCulture);
            return (datetime.Subtract(_unixStartDate).TotalSeconds,
                datetime.ToUniversalTime().Subtract(_unixStartDate).TotalSeconds);
        }
    }
}
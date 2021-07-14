using System.Collections.Generic;
using System.Text.Json;
using Pipeline.PipelineCore.StepsCore;

namespace Pipeline.Steps
{
    public class SerializationStep : RegularStep<Dictionary<string, string>, string>
    {
        public override string ProcessItem(Dictionary<string, string> parsedLogLine)
        {
            return JsonSerializer.Serialize(parsedLogLine);
        }
    }
}

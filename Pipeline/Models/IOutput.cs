using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using Pipeline.Models.Base;

namespace Pipeline.Models
{
    public interface IOutput<TOut> : IPipelineBlock
    {
        IAsyncEnumerable<TOut> ReadFromChannelAsync(CancellationToken ct);
        void AssignInputChannel(ChannelReader<TOut> channel);
    }
}

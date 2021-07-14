using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Pipeline.Models.Base;

namespace Pipeline.Models
{
    public interface IStep<TIn, TOut> : IPipelineBlock
    {
        IAsyncEnumerable<TIn> ReadFromChannelAsync(CancellationToken ct);
        TOut ProcessItem(TIn item);
        Task WriteToChannelAsync(TOut item, CancellationToken ct);
        void AssignInputChannel(ChannelReader<TIn> channel);
        void AssignOutputChannel(ChannelWriter<TOut> channel);
    }
}

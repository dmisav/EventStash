using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Pipeline.Models.Base;
using Pipeline.PipelineCore;

namespace Pipeline.Models
{
    public interface IStep<TIn, TOut> : IPipelineBlock
    {
        IAsyncEnumerable<TIn> ReadFromChannelAsync(CancellationTokenWrapper tokenWrapper);
        TOut ProcessItem(TIn item);
        ValueTask WriteToChannelAsync(TOut item, CancellationTokenWrapper tokenWrapper);
        void AssignInputChannel(ChannelReader<TIn> channel);
        void AssignOutputChannel(ChannelWriter<TOut> channel);
    }
}

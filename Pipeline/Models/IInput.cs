using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Pipeline.Models.Base;
using Pipeline.PipelineCore;

namespace Pipeline.Models
{
    public interface IInput<TIn> : IPipelineBlock
    {
        ValueTask WriteToChannelAsync(TIn item, CancellationTokenWrapper tokenWrapper);
        void AssignOutputChannel(ChannelWriter<TIn> channel);
    }
}

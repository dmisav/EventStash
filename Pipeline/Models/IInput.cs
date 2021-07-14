using Pipeline.Models.Base;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Pipeline.Models
{
    public interface IInput<TIn> : IPipelineBlock
    {
        ValueTask WriteToChannelAsync(TIn item, CancellationToken ct);
        void AssignOutputChannel(ChannelWriter<TIn> channel);
    }
}

using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Pipeline.Models.Base;

namespace Pipeline.Models
{
    public interface IInput<TIn> : IPipelineBlock
    {
        Task WriteToChannelAsync(TIn item, CancellationToken ct);
        void AssignOutputChannel(ChannelWriter<TIn> channel);
    }
}

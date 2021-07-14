using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Pipeline.Models
{
    public interface IInput<TIn>
    {
        ValueTask WriteToChannelAsync(TIn item, CancellationToken ct);
        Task StartRoutine(CancellationToken ct);
        void AssignOutputChannel(ChannelWriter<TIn> channel);
    }
}

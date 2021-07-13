using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Pipeline.Models
{
    public interface IOutput<TOut>
    {
        IAsyncEnumerable<TOut> ReadFromChannelAsync(CancellationToken ct);
        Task StartRoutine(CancellationToken ct);
        void AssignInputChannel(ChannelReader<TOut> channel);
    }
}

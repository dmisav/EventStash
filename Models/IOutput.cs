using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pipeline.Models
{
    public interface IOutput<TOut>
    {
        IAsyncEnumerable<TOut> ReadFromChannelAsync(CancellationToken ct);
        Task StartRoutine(CancellationToken ct);
    }
}

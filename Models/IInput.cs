using System.Threading;
using System.Threading.Tasks;

namespace Pipeline.Models
{
    public interface IInput<TIn>
    {
        Task WriteToChannelAsync(TIn item, CancellationToken ct);
        Task StartRoutine(CancellationToken ct);
    }
}

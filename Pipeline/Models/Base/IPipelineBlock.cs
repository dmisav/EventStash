using System.Threading;
using System.Threading.Tasks;

namespace Pipeline.Models.Base
{
    public interface IPipelineBlock
    {
        Task StartRoutine(CancellationToken ct);
    }
}

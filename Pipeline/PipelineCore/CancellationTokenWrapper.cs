using System.Threading;

namespace Pipeline.PipelineCore
{
    public class CancellationTokenWrapper
    {
        public CancellationTokenWrapper(CancellationToken token) => Token = token;

        public CancellationToken Token { get; }
    }
}
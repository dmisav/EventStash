using System.Threading;

namespace Pipeline.Models
{
    public interface IPipeline
    {
        IPipeline AddInput<TIn>(IInput<TIn> input);
        IPipeline AddStep<TIn, TOut>(IStep<TIn, TOut> step);
        IPipeline AddOutput<TOut>(IOutput<TOut> output);
        IPipeline Create(CancellationToken ct);
        void Start();
    }
}

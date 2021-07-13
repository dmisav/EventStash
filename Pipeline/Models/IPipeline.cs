using System.Threading;
using System.Threading.Tasks;
using Pipeline.Configuration;

namespace Pipeline.Models
{
    public interface IPipeline
    {
        IPipeline AddInput<TIn>(IInput<TIn> input);
        IPipeline AddStep<TIn, TOut>(IStep<TIn, TOut> step);
        IPipeline AddOutput<TOut>(IOutput<TOut> output);
        IPipeline Create(ScalingOptions scalingOptions);
        Task Start(CancellationToken ct);
    }
}

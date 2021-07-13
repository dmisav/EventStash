using System.Threading.Channels;

namespace Pipeline.PipelineCore.StepsCore
{
    public abstract class RegularStep<TIn, TOut> : BaseStep<TIn, TOut>
    {
        public RegularStep(ChannelReader<TIn> channelIn, ChannelWriter<TOut> channelOut) : base(channelIn, channelOut) { }
    }
}

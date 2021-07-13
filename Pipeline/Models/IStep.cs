using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

public interface IStep<TIn, TOut>
{
	IAsyncEnumerable<TIn> ReadFromChannelAsync(CancellationToken ct);
	TOut ProcessItem(TIn item);
	Task WriteToChannelAsync(TOut item, CancellationToken ct);
	Task StartRoutine(CancellationToken ct);
    void AssignInputChannel(ChannelReader<TIn> channel);
    void AssignOutputChannel(ChannelWriter<TOut> channel);
}

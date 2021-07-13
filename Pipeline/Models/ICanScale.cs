using Pipeline.Configuration;

namespace Pipeline.Models
{
    public interface ICanScale
    {
        int ScaleChannelOptions(ScalingOptions options);
        int UnscaleChannelOptions(ScalingOptions options);
    }
}

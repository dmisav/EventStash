using Pipeline.Configuration;
using System;

namespace Pipeline.Helpers
{
    public static class AutoScalerHelper
    {
        public static bool IsScalingPossible(ref ScalingOptions options)
        {
            var currParallelCount = options.ParallelCount;
            var newParallelCount = options.ScalingFactor * currParallelCount;

            if (options.MaxParallelCount != -1)
                newParallelCount = Math.Min(newParallelCount, options.MaxParallelCount);

            var currQueueSize = options.ParallelQueueSize;
            var newQueueSize = options.ScalingFactor * currQueueSize;

            if (options.MaxParallelQueueSize != -1)
                newQueueSize = Math.Min(newQueueSize, options.MaxParallelQueueSize);

            options.ParallelCount = newParallelCount;
            options.ParallelQueueSize = newQueueSize;

            return currParallelCount != newParallelCount;
        }

        public static bool IsUnscalingPossible(ref ScalingOptions options)
        {
            var currParallelCount = options.ParallelCount;
            var newParallelCount = currParallelCount / options.ScalingFactor;

            var currQueueSize = options.ParallelQueueSize;
            var newQueueSize = currQueueSize / options.ScalingFactor;

            options.ParallelCount = Math.Max(newParallelCount, 1);
            options.ParallelQueueSize = newQueueSize == 0 ? currQueueSize : newQueueSize;

            return currParallelCount != newParallelCount;
        }
    }
}

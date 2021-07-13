using Pipeline.Configuration;
using System;

namespace Pipeline.Helpers
{
    public static class AutoScalerHelper
    {
        public static int ScaleParallelCount(ScalingOptions options)
        {
            var currParallelCount = options.ParallelCount;
            var newParallelCount = options.ScalingFactor * currParallelCount;

            if (options.MaxParallelCount != -1)
                newParallelCount = Math.Min(newParallelCount, options.MaxParallelCount);

            return newParallelCount;
        }

        public static int UnscaleParallelCount(ScalingOptions options)
        {
            var currParallelCount = options.ParallelCount;
            var newParallelCount = currParallelCount / options.ScalingFactor;

            return Math.Max(newParallelCount, 1);
        }
    }
}

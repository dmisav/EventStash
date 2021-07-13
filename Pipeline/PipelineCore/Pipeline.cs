using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Pipeline.Configuration;
using Pipeline.Models;

namespace Pipeline.PipelineCore
{
    public class Pipeline : IPipeline
    {
        public IPipeline AddInput<TIn>(IInput<TIn> input)
        {
            throw new NotImplementedException();
        }

        public IPipeline AddStep<TIn, TOut>(IStep<TIn, TOut> step)
        {
            throw new NotImplementedException();
        }

        public IPipeline AddOutput<TOut>(IOutput<TOut> output)
        {
            throw new NotImplementedException();
        }

        public IPipeline Create(ScalingOptions scalingOptions)
        {
            throw new NotImplementedException();
        }

        public Task Start(CancellationToken ct)
        {
            throw new NotImplementedException();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Pipeline.Models;
using Pipeline.Models.Base;

namespace Pipeline.PipelineCore
{
    public class Pipeline : IPipeline
    {
        private List<object> _channels = new List<object>();
        private IPipelineBlock _input;
        private List<IPipelineBlock> _steps = new List<IPipelineBlock>();
        private IPipelineBlock _output;
        private List<Type> _pipelineTypes = new List<Type>();
        private List<Task> _pipelineTasks = new List<Task>();

        public IPipeline AddInput<TIn>(IInput<TIn> input)
        {
            _input = input;
            var channel = Channel.CreateUnbounded<TIn>();
            input.AssignOutputChannel(channel.Writer);
            _channels.Add(channel);
            _pipelineTypes.Add(typeof(TIn));

            return this;
        }

        public IPipeline AddStep<TIn, TOut>(IStep<TIn, TOut> step)
        {
            if (_input == null)
                throw new Exception("Can't add steps while pipeline has no input.");

            if (_pipelineTypes.Last() != typeof(TIn))
                throw new Exception("Can't add step due to previous block output and current step input type mismatch");

            _steps.Add(step);
            var channel = Channel.CreateUnbounded<TOut>();
            var lastChannel = (Channel<TIn>)_channels.Last();
            step.AssignInputChannel(lastChannel.Reader);
            step.AssignOutputChannel(channel.Writer);
            _channels.Add(channel);
            _pipelineTypes.Add(typeof(TOut));

            return this;
        }

        public IPipeline AddOutput<TOut>(IOutput<TOut> output)
        {
            if (_input == null)
                throw new Exception("Can't add steps while pipeline has no input.");

            if (_pipelineTypes.Last() != typeof(TOut))
                throw new Exception("Can't add step due to previous block output and current step input type mismatch");

            _output = output;
            var lastChannel = (Channel<TOut>)_channels.Last();
            output.AssignInputChannel(lastChannel.Reader);

            return this;
        }

        public IPipeline Create(CancellationToken ct)
        {
            var inputTask = _input.StartRoutine(ct);
            _pipelineTasks.Add(inputTask);

            foreach (var step in _steps)
            {
                var stepTask = step.StartRoutine(ct);
                _pipelineTasks.Add(stepTask);
            }

            var outputTask = _output.StartRoutine(ct);
            _pipelineTasks.Add(outputTask);

            return this;
        }

        public void Start()
        {
            foreach (var task in _pipelineTasks)
            {
                task.Start();
            }
        }
    }
}

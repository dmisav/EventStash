using Pipeline.Configuration;
using Pipeline.Monitoring;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pipeline.AutoScaling
{
    public interface ICanCount
    {
        int GetCount();
    }

    public class AutoScaler
    {
        private readonly ICanCount _canCount;
        private readonly int[] _trend;
        private readonly int _trendCount;
        private int _index;

        public AutoScaler(ICanCount canCount, int trendCount)
        {
            _canCount = canCount;
            _trendCount = trendCount;
            _trend = new int[_trendCount];
            _index = 0;
        }

        public State CalculateState()
        {
            var curr = _canCount.GetCount();
            _trend[_index++] = curr;



            var currState = AnalyzeTrend(_trend);

            return currState;
        }

        private State AnalyzeTrend(int[] trend)
        {
            var localTrend = new int[_trendCount];

            Array.Copy(trend, localTrend, _trendCount);

            bool hasIncreased = false, hasDecreased = false;

            for (int i = 1; i < localTrend.Length; i++)
            {
                hasIncreased = hasIncreased || localTrend[i] > localTrend[i - 1];
                hasDecreased = hasDecreased || localTrend[i] < localTrend[i - 1];
            }

            if (hasIncreased == hasDecreased)
                return State.Steady;

            return hasIncreased ? State.Growing : State.Sinking;
        }
    }

    public enum State
    { 
        Growing,
        Sinking,
        Steady
    }
}

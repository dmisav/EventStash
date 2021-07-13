using Pipeline.Models;
using System;
using System.Collections.Generic;

namespace Pipeline.AutoScaling
{
    public class TrendChecker
    {
        private readonly List<int> _trend;
        private readonly int _trendCount;

        public EventHandler<State> StateActionRequired;

        public TrendChecker(int trendCount)
        {
            _trendCount = trendCount;
            _trend = new List<int>(trendCount);
        }

        public void UpdateCount(int count)
        {
            _trend.Add(count);

            if (_trend.Count == _trendCount)
            {
                var currState = CalculateState();

                if (currState == State.Steady)
                    return;

                StateActionRequired(this, currState);
            }

        }

        public State CalculateState()
        {
            var copy = new List<int>(_trend);
            _trend.Clear();

            var currState = AnalyzeTrend(copy);

            return currState;
        }

        private State AnalyzeTrend(List<int> trend)
        {
            bool hasIncreased = false, hasDecreased = false;

            for (int i = 1; i < trend.Count; i++)
            {
                hasIncreased = hasIncreased || trend[i] > trend[i - 1];
                hasDecreased = hasDecreased || trend[i] < trend[i - 1];
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

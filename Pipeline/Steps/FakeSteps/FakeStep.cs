using Pipeline.PipelineCore.StepsCore;

namespace Pipeline.FakeSteps
{
    public class FakeStep : RegularStep<string, string>
    {
        public readonly int _stepNumber;

        public FakeStep(int stepNumber)
        {
            _stepNumber = stepNumber;
        }

        public override string ProcessItem(string item)
        {
            return $"{item} Fake step #{_stepNumber} completed;";
        }
    }
}

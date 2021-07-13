using Pipeline.PipelineCore.StepsCore;

namespace Pipeline.FakeSteps
{
    public class FakeStep : RegularStep<string, string>
    {
        public override string ProcessItem(string item)
        {
            return $"{item} Fake step completed;";
        }
    }
}

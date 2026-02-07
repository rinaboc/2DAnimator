namespace Assets.Scripts.States
{
    public class ParameterTimelineState
    {
        public ParameterStates Parameters { get; set; }
        public TimelineState Timeline { get; set; }

        public ParameterTimelineState()
        {
            Parameters = new();
            Timeline = new();
        }

        public ParameterTimelineState Clone() => new()
        {
            Parameters = Parameters.Clone(),
            Timeline = Timeline.Clone()
        };
    }
}

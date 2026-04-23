using Assets.Scripts.States;
using Assets.Scripts.Utility.MVI;

public class ParameterTimelineReducer : IReducer<ParameterTimelineState>
{
    private IReducer<ParameterStates> _parameterReducer;
    private IReducer<TimelineState> _timelineReducer;

    public ParameterTimelineReducer(IReducer<ParameterStates> parameterReducer, IReducer<TimelineState> timelineReducer)
    {
        _parameterReducer = parameterReducer;
        _timelineReducer = timelineReducer;
    }

    public ParameterTimelineState Reduce(ParameterTimelineState previous, IIntent intent)
    {
        var next = previous.Copy();
        next.Parameters = _parameterReducer.Reduce(previous.Parameters, intent);
        next.Timeline = _timelineReducer.Reduce(previous.Timeline, intent);

        if (ReferenceEquals(next.Parameters, previous.Parameters) && ReferenceEquals(next.Timeline, previous.Timeline))
            return previous;

        return next;
    }

    public ParameterTimelineState Update(ParameterTimelineState previous, IModelContext context)
    {
        var next = previous.Copy();
        next.Parameters = _parameterReducer.Update(previous.Parameters, context);
        next.Timeline = _timelineReducer.Update(previous.Timeline, context);
        return next;
    }
}

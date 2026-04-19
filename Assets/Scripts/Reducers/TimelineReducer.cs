using Assets.Scripts.States;
using Assets.Scripts.Utility.MVI;

public class TimelineReducer : IReducer<TimelineState>
{
    public TimelineState Reduce(TimelineState previous, IIntent intent)
    {
        return intent switch
        {
            TimelineSettingsOpenIntent _ => ReduceOpenTimelineSettings(previous),
            StartTimelineSliderDragIntent _ => previous.Clone(),
            StartTimelineParameterDragIntent _ => previous.Clone(),
            _ => previous
        };
    }

    private TimelineState ReduceOpenTimelineSettings(TimelineState previous)
    {
        var next = previous.Clone();
        next.IsSettingsOpen = !next.IsSettingsOpen;
        return next;
    }

    public TimelineState Update(TimelineState previous, IModelContext context)
    {
        var next = new TimelineState(context, previous);
        return next;
    }
}

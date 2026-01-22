using System;
using Assets.Scripts.States;
using Assets.Scripts.Utility.MVI;

public class TimelineReducer : IReducer<TimelineState>
{
    public TimelineState Reduce(TimelineState previous, IIntent intent)
    {
        return intent switch
        {
            TimelineOpenIntent _ => ReduceOpenTimeline(previous),
            UpdateFramePerSecIntent update => ReduceUpdateFramePerSec(update, previous),
            UpdateMaxFramesIntent update => ReduceUpdateMaxFrames(update, previous),
            TimelineSettingsOpenIntent _ => ReduceOpenTimelineSettings(previous),
            _ => previous
        };
    }

    private TimelineState ReduceOpenTimelineSettings(TimelineState previous)
    {
        var next = previous.Clone();
        next.IsSettingsOpen = !next.IsSettingsOpen;
        return next;
    }

    private TimelineState ReduceUpdateMaxFrames(UpdateMaxFramesIntent update, TimelineState previous)
    {
        var next = previous.Clone();
        next.MaxFrames = update.MaxFrames;
        return next;
    }

    private TimelineState ReduceUpdateFramePerSec(UpdateFramePerSecIntent update, TimelineState previous)
    {
        var next = previous.Clone();
        next.FramePerSec = update.FPS;
        return next;
    }

    private TimelineState ReduceOpenTimeline(TimelineState previous)
    {
        var next = previous.Clone();
        next.IsOpen = !next.IsOpen;
        return next;
    }
}

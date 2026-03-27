using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.States;
using Assets.Scripts.Utility.MVI;

public class TimelineReducer : IReducer<TimelineState>
{
    public TimelineState Reduce(TimelineState previous, IIntent intent)
    {
        return intent switch
        {
            // TimelineOpenIntent _ => ReduceOpenTimeline(previous),
            // UpdateFramePerSecIntent update => ReduceUpdateFramePerSec(update, previous),
            // UpdateMaxFramesIntent update => ReduceUpdateMaxFrames(update, previous),
            // TimelineSettingsOpenIntent _ => ReduceOpenTimelineSettings(previous),
            // CreateParameterIntent create => ReduceCreateParameter(previous, create),
            // DeleteSelectedParameterIntent delete => ReduceDeleteSelectedParameter(previous, delete),
            // SelectKeyframeIntent select => ReduceSelectKeyframe(previous, select),
            // DeleteKeyframeIntent _ => ReduceDeleteKeyframe(previous),
            // CurrentFrameChangedIntent change => ReduceCurrentFrameChanged(change, previous),
            // TimelineParameterSliderChangedIntent change => ReduceTimelineParameterSliderChanged(change, previous),
            // InitializeProjectIntent init => ReduceInitializeProject(previous, init),
            // SelectParameterIntent select => ReduceSelectParameter(previous, select),
            // StartTimelineSliderDragIntent _ => previous.Clone(),
            // StartTimelineParameterDragIntent _ => previous.Clone(),
            _ => previous
        };
    }

    private TimelineState ReduceSelectParameter(TimelineState previous, SelectParameterIntent select)
    {
        var next = previous.Clone();
        next.SelectedParamID = select.ParamID;
        return next;
    }

    private TimelineState ReduceInitializeProject(TimelineState previous, InitializeProjectIntent init)
    {
        var next = new TimelineState
        {
            FramePerSec = init.SaveData.AnimationSetting?.framePerSec ?? 24,
            MaxFrames = init.SaveData.AnimationSetting?.maxFrames ?? 24,
        };

        foreach (Parameter parameter in init.SaveData.Parameters)
        {
            next.Keyframes.Add(parameter.ID, new Dictionary<Guid, KeyframeState>());
        }

        foreach (KeyFrame keyFrame in init.SaveData.KeyFrames)
        {
            next.Keyframes[keyFrame.ParamID][keyFrame.ID] = new KeyframeState()
            {
                Frame = keyFrame.Frame,
                IsSelected = false,
                Value = keyFrame.ParamValue
            };
        }

        return next;
    }

    private TimelineState ReduceDeleteKeyframe(TimelineState previous)
    {
        var next = previous.Clone();

        if (next.SelectedKeyframe.Item1 == Guid.Empty || next.SelectedKeyframe.Item2 == Guid.Empty) return next;

        next.Keyframes[next.SelectedKeyframe.Item1].Remove(next.SelectedKeyframe.Item2);
        next.SelectedKeyframe = new(Guid.Empty, Guid.Empty);

        return next;
    }

    private TimelineState ReduceTimelineParameterSliderChanged(TimelineParameterSliderChangedIntent change, TimelineState previous)
    {
        var next = previous.Clone();
        if (next.Keyframes[change.ParamID].Any(kf => kf.Value.Frame == next.CurrentFrame))
        {
            var update = next.Keyframes[change.ParamID].FirstOrDefault(kf => kf.Value.Frame == next.CurrentFrame);
            update.Value.Value = change.Value;
            return next;
        }

        next.Keyframes[change.ParamID][Guid.NewGuid()] = new KeyframeState()
        {
            Frame = next.CurrentFrame,
            IsSelected = false,
            Value = change.Value
        };
        return next;
    }

    private TimelineState ReduceCurrentFrameChanged(CurrentFrameChangedIntent change, TimelineState previous)
    {
        var next = previous.Clone();
        next.CurrentFrame = change.Frame;
        return next;
    }

    private TimelineState ReduceSelectKeyframe(TimelineState previous, SelectKeyframeIntent select)
    {
        var next = previous.Clone();

        if (next.SelectedKeyframe.Item1 != Guid.Empty && next.SelectedKeyframe.Item2 != Guid.Empty)
            next.Keyframes[next.SelectedKeyframe.Item1][next.SelectedKeyframe.Item2].IsSelected = false;

        foreach (Guid paramID in next.Keyframes.Keys)
        {
            foreach (Guid keyID in next.Keyframes[paramID].Keys)
            {
                if (keyID == select.ID)
                {
                    next.SelectedKeyframe = new(paramID, keyID);
                    next.Keyframes[paramID][keyID].IsSelected = true;
                    return next;
                }
            }
        }

        next.SelectedKeyframe = new(Guid.Empty, Guid.Empty);
        return next;
    }

    private TimelineState ReduceDeleteSelectedParameter(TimelineState previous, DeleteSelectedParameterIntent delete)
    {
        if (previous.SelectedParamID == Guid.Empty) return previous;

        var next = previous.Clone();
        next.Keyframes.Remove(previous.SelectedParamID);
        next.SelectedParamID = Guid.Empty;
        return next;
    }

    private TimelineState ReduceCreateParameter(TimelineState previous, CreateParameterIntent create)
    {
        var next = previous.Clone();
        next.Keyframes.Add(create.ParamID, new Dictionary<Guid, KeyframeState>());
        return next;
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

    public TimelineState Update(TimelineState previous, IModelContext context)
    {
        return previous;
    }
}

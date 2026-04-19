using System;
using System.Linq;
using Assets.Scripts.Utility.MVI;

public class TimelineCommandHandler : ICommandHandler
{
    public Action Execute(IIntent intent, IModelContext context)
    {
        return intent switch
        {
            TimelineOpenIntent _ => ExecuteOpenTimeline(context),
            UpdateFramePerSecIntent update => ExecuteUpdateFramePerSec(update, context),
            UpdateMaxFramesIntent update => ExecuteUpdateMaxFrames(update, context),
            CurrentFrameChangedIntent change => ExecuteCurrentFrameChanged(change, context),
            TimelineParameterSliderChangedIntent change => ExecuteTimelineParameterSliderChanged(change, context),
            DeleteKeyframeIntent _ => ExecuteDeleteKeyframe(context),
            SelectKeyframeIntent select => ExecuteSelectKeyframe(select, context),
            InitializeProjectIntent init => ExecuteInitializeProject(init, context),
            _ => null
        };
    }

    private Action ExecuteSelectKeyframe(SelectKeyframeIntent select, IModelContext context)
    {
        if (context.SessionInfo.SelectedKeyframeID == select.ID) return null;

        var previousID = context.SessionInfo.SelectedKeyframeID;
        context.SessionInfo.SelectedKeyframeID = select.ID;

        return () => context.SessionInfo.SelectedKeyframeID = previousID;
    }

    private Action ExecuteDeleteKeyframe(IModelContext context)
    {
        if (context.SessionInfo.SelectedKeyframeID == Guid.Empty) return null;

        context.KeyFrames.TryGet(context.SessionInfo.SelectedKeyframeID, out KeyFrame keyFrame);
        context.KeyFrames.Remove(keyFrame.ID);

        context.SessionInfo.SelectedKeyframeID = Guid.Empty;

        return () =>
        {
            context.KeyFrames.Register(keyFrame);
            context.SessionInfo.SelectedKeyframeID = keyFrame.ID;
        };
    }

    private Action ExecuteTimelineParameterSliderChanged(TimelineParameterSliderChangedIntent change, IModelContext context)
    {
        var keyframesOfParam = context.KeyFrames.GetKeyFramesOfParam(change.ParamID);

        if (keyframesOfParam.Any(kf => kf.Frame == context.SessionInfo.CurrentFrame))
        {
            var update = keyframesOfParam.FirstOrDefault(kf => kf.Frame == context.SessionInfo.CurrentFrame);
            var previousValue = update.ParamValue;
            update.ParamValue = change.Value;

            return () => update.ParamValue = previousValue;
        }

        KeyFrame keyframe = new(change.ParamID, change.Value, context.SessionInfo.CurrentFrame) { ID = Guid.NewGuid() };
        context.KeyFrames.Register(keyframe);
        AnimationManager.Instance.InterpolateParameter(keyframe.ParamValue, keyframe.ParamID, context);

        return () => context.KeyFrames.Remove(keyframe.ID);
    }

    private Action ExecuteUpdateMaxFrames(UpdateMaxFramesIntent update, IModelContext context)
    {
        if (context.GeneralSettings.MaxFrames == update.MaxFrames) return null;

        var previousMaxFrames = context.GeneralSettings.MaxFrames;
        context.GeneralSettings.MaxFrames = update.MaxFrames;
        return () => context.GeneralSettings.MaxFrames = previousMaxFrames;
    }

    private Action ExecuteUpdateFramePerSec(UpdateFramePerSecIntent update, IModelContext context)
    {
        if (context.GeneralSettings.FramePerSec == update.FPS) return null;

        var previousFPS = context.GeneralSettings.FramePerSec;
        context.GeneralSettings.FramePerSec = update.FPS;
        return () => context.GeneralSettings.FramePerSec = previousFPS;
    }

    private Action ExecuteInitializeProject(InitializeProjectIntent init, IModelContext context)
    {
        context.GeneralSettings.MaxFrames = init.SaveData.AnimationSetting?.maxFrames ?? 24;
        context.GeneralSettings.FramePerSec = init.SaveData.AnimationSetting?.framePerSec ?? 24;

        context.KeyFrames.Clear();

        foreach (var item in init.SaveData.KeyFrames)
        {
            context.KeyFrames.Register(item);
        }

        return null;
    }
    private Action ExecuteCurrentFrameChanged(CurrentFrameChangedIntent change, IModelContext context)
    {
        static async void AnimateTimeline(int frame, IModelContext context)
        {
            await AnimationManager.Instance.AnimateTimeline(frame, context);
        }

        context.SessionInfo.CurrentFrame = change.Frame;
        AnimateTimeline(change.Frame, context);
        return null;
    }

    private Action ExecuteOpenTimeline(IModelContext context)
    {
        context.SessionInfo.TimelineVisibility = !context.SessionInfo.TimelineVisibility;
        ParameterManager.Instance.ParameterWidgetVisibility = !context.SessionInfo.TimelineVisibility;

        return () =>
        {
            context.SessionInfo.TimelineVisibility = !context.SessionInfo.TimelineVisibility;
            ParameterManager.Instance.ParameterWidgetVisibility = !context.SessionInfo.TimelineVisibility;
        };
    }
}
using System;
using System.Linq;
using Assets.Scripts.States;
using Assets.Scripts.Utility.MVI;

public class TimelineCommandHandler : ICommandHandler
{
    public void Execute(IIntent intent, object state, IModelContext context)
    {
        switch (intent)
        {
            case TimelineOpenIntent _: ExecuteOpenTimeline(state, context); break;
            case CurrentFrameChangedIntent change: ExecuteCurrentFrameChanged(change, state, context); break;
            case TimelineParameterSliderChangedIntent change: ExecuteTimelineParameterSliderChanged(change, state, context); break;
            case DeleteKeyframeIntent _: ExecuteDeleteKeyframe(state, context); break;
            case InitializeProjectIntent init: ExecuteInitializeProject(init, state, context); break;
        }
    }

    private void ExecuteInitializeProject(InitializeProjectIntent init, object state, IModelContext context)
    {
        context.GeneralSettings.MaxFrames = init.SaveData.AnimationSetting?.maxFrames ?? 24;
        context.GeneralSettings.FramePerSec = init.SaveData.AnimationSetting?.framePerSec ?? 24;

        context.KeyFrames.Clear();

        foreach (var item in init.SaveData.KeyFrames)
        {
            context.KeyFrames.Register(item);
        }
    }

    private void ExecuteDeleteKeyframe(object state, IModelContext context)
    {
        var keyframeState = ((TimelineState)state).Keyframes;

        foreach (KeyFrame kf in context.KeyFrames.GetAll().ToList())
        {
            if (!keyframeState[kf.ParamID].ContainsKey(kf.ID))
            {
                context.KeyFrames.Remove(kf.ID);
            }
        }
    }

    private void ExecuteCurrentFrameChanged(CurrentFrameChangedIntent change, object state, IModelContext context)
    {
        context.GeneralSettings.CurrentFrame = change.Frame;
        AnimationManager.Instance.AnimateTimeline(change.Frame);
    }

    private void ExecuteTimelineParameterSliderChanged(TimelineParameterSliderChangedIntent change, object state, IModelContext context)
    {
        AnimationManager.Instance.InterpolateParameter(change.Value, change.ParamID);
        KeyFrame keyFrame = new(change.ParamID, change.Value, context.GeneralSettings.CurrentFrame, autoRegister: false);
        keyFrame.ID = change.KeyID;
        context.KeyFrames.Register(keyFrame);
    }

    private void ExecuteOpenTimeline(object state, IModelContext context)
    {
        ParameterManager.Instance.ParameterWidgetVisibility = !ParameterManager.Instance.ParameterWidgetVisibility;
    }
}
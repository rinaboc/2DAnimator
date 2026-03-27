using System;
using System.Linq;
using System.Threading.Tasks;
using Assets.Scripts.States;
using Assets.Scripts.Utility.MVI;

public class TimelineCommandHandler : ICommandHandler
{
    public Action Execute(IIntent intent, IModelContext context)
    {
        return intent switch
        {
            _ => null
        };
    }

    /**
    public void Execute(IIntent intent, object state, IModelContext context)
    {
        var timelineState = (state as ParameterTimelineState)?.Timeline;

        switch (intent)
        {
            case TimelineOpenIntent _: ExecuteOpenTimeline(timelineState); break;
            case CurrentFrameChangedIntent _: ExecuteCurrentFrameChanged(timelineState, context); break;
            case TimelineParameterSliderChangedIntent _: ExecuteKeyframeStateChanged(timelineState, context); break;
            case DeleteKeyframeIntent _: ExecuteKeyframeStateChanged(timelineState, context); break;
            case InitializeProjectIntent init: ExecuteInitializeProject(init, context); break;
            case UpdateFramePerSecIntent _: ExecuteUpdateFramePerSec(timelineState, context); break;
            case UpdateMaxFramesIntent _: ExecuteUpdateMaxFrames(timelineState, context); break;
        }
    }

    private void ExecuteUpdateMaxFrames(TimelineState state, IModelContext context)
    {
        if (state == null) return;
        context.GeneralSettings.MaxFrames = state.MaxFrames;
    }

    private void ExecuteUpdateFramePerSec(TimelineState state, IModelContext context)
    {
        if (state == null) return;
        context.GeneralSettings.FramePerSec = state.FramePerSec;
    }

    private void ExecuteInitializeProject(InitializeProjectIntent init, IModelContext context)
    {
        context.GeneralSettings.MaxFrames = init.SaveData.AnimationSetting?.maxFrames ?? 24;
        context.GeneralSettings.FramePerSec = init.SaveData.AnimationSetting?.framePerSec ?? 24;

        context.KeyFrames.Clear();

        foreach (var item in init.SaveData.KeyFrames)
        {
            context.KeyFrames.Register(item);
        }
    }

    private void ExecuteKeyframeStateChanged(TimelineState state, IModelContext context)
    {
        if (state == null) return;
        var keyframes = context.KeyFrames.GetAll().ToList();
        foreach ((var paramID, var keyframeLine) in state.Keyframes)
        {
            foreach ((var keyID, var keyframeState) in keyframeLine)
            {
                if (!keyframes.Any(kf => kf.ID == keyID)) // create action
                {
                    AnimationManager.Instance.InterpolateParameter(keyframeState.Value, paramID, context);
                    KeyFrame keyFrame = new(paramID, keyframeState.Value, keyframeState.Frame) { ID = keyID };
                    context.KeyFrames.Register(keyFrame);
                }
                else // update action
                {
                    keyframes.Remove(keyframes.FirstOrDefault(kf => kf.ID == keyID));
                    context.KeyFrames.TryGet(keyID, out KeyFrame keyFrame);
                    keyFrame.Frame = keyframeState.Frame;
                    keyFrame.ParamValue = keyframeState.Value;
                    keyFrame.ParamID = paramID;

                }
            }
        }

        foreach (KeyFrame kf in keyframes) // delete action 
        {
            context.KeyFrames.Remove(kf.ID);
        }

    }

    private async void ExecuteCurrentFrameChanged(TimelineState state, IModelContext context)
    {
        if (state == null) return;
        context.GeneralSettings.CurrentFrame = state.CurrentFrame;
        await AnimationManager.Instance.AnimateTimeline(state.CurrentFrame, context);
    }

    private void ExecuteOpenTimeline(TimelineState state)
    {
        if (state == null) return;
        ParameterManager.Instance.ParameterWidgetVisibility = !state.IsOpen;
    }
    **/
}
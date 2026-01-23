using System;
using Assets.Scripts.Utility.MVI;

public class TimelineCommandHandler : ICommandHandler
{
    public void Execute(IIntent intent, object state, IModelContext context)
    {
        switch (intent)
        {
            case TimelineOpenIntent _: ExecuteOpenTimeline(state, context); break;
        }
    }

    private void ExecuteOpenTimeline(object state, IModelContext context)
    {
        ParameterManager.Instance.ParameterWidgetVisibility = !ParameterManager.Instance.ParameterWidgetVisibility;
    }
}
using System;
using Assets.Scripts.Utility.MVI;

public class OperationCommandHandler : ICommandHandler
{
    public Action Execute(IIntent intent, IModelContext context)
    {
        return intent switch
        {
            OpenProjectIntent open => ExecuteOpenProject(open),
            SaveProjectIntent save => ExecuteSaveProject(save, context),
            _ => ExecuteDefault(context, intent)
        };
    }

    private Action ExecuteDefault(IModelContext context, IIntent intent)
    {
        int prevRedoCount = context.SessionInfo.RedoCount;

        if (IntentHelper.IsUndoableIntent(intent))
        {
            context.SessionInfo.UndoCount++;
            if (context.SessionInfo.UndoCount > context.GeneralSettings.HistoryLimit)
                context.SessionInfo.UndoCount = context.GeneralSettings.HistoryLimit;

            if (context.SessionInfo.HistoryReset)
                context.SessionInfo.RedoCount = 0;
            else
                context.SessionInfo.RedoCount--;

        }

        return () =>
        {
            if (!IntentHelper.IsUndoableIntent(intent)) return;
            context.SessionInfo.UndoCount--;
            context.SessionInfo.RedoCount++;
        };
    }

    private Action ExecuteSaveProject(SaveProjectIntent save, IModelContext context)
    {
        ProjectManager.Instance.SaveProject(save.Path, context);

        return null;
    }

    private Action ExecuteOpenProject(OpenProjectIntent open)
    {
        ProjectManager.Instance.LoadProject(open.Path);

        return null;
    }
}

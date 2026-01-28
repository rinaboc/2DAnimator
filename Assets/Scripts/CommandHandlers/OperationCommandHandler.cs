using Assets.Scripts.Utility.MVI;

public class OperationCommandHandler : ICommandHandler
{
    public void Execute(IIntent intent, object state, IModelContext context)
    {
        switch (intent)
        {
            case OpenProjectIntent open: ExecuteOpenProject(open, state, context); break;
            case SaveProjectIntent save: ExecuteSaveProject(save, state, context); break;
        }
    }

    private void ExecuteSaveProject(SaveProjectIntent save, object state, IModelContext context)
    {
        ProjectManager.Instance.SaveProject(save.Path);
    }

    private void ExecuteOpenProject(OpenProjectIntent open, object state, IModelContext context)
    {
        ProjectManager.Instance.LoadProject(open.Path);
    }
}

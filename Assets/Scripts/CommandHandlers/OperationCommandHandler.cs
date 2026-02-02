using Assets.Scripts.Utility.MVI;

public class OperationCommandHandler : ICommandHandler
{
    public void Execute(IIntent intent, object state, IModelContext context)
    {
        switch (intent)
        {
            case OpenProjectIntent open: ExecuteOpenProject(open); break;
            case SaveProjectIntent save: ExecuteSaveProject(save, context); break;
        }
    }

    private void ExecuteSaveProject(SaveProjectIntent save, IModelContext context)
    {
        ProjectManager.Instance.SaveProject(save.Path, context);
    }

    private void ExecuteOpenProject(OpenProjectIntent open)
    {
        ProjectManager.Instance.LoadProject(open.Path);
    }
}

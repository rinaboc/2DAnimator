namespace Assets.Scripts.Utility.MVI
{
    public interface ICommandHandler
    {
        bool CanHandle(IIntent intent);
        void Execute(IIntent intent, object state, IModelContext context);
    }
}

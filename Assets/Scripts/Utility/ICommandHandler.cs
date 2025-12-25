namespace Assets.Scripts.Utility
{
    public interface ICommandHandler
    {
        bool CanHandle(IIntent intent);
        void Execute(IIntent intent, object state, IModelContext context);
    }
}

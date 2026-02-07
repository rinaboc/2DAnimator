namespace Assets.Scripts.Utility.MVI
{
    public interface ICommandHandler
    {
        void Execute(IIntent intent, object state, IModelContext context);
    }
}

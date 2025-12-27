namespace Assets.Scripts.Utility.MVI
{
    public interface IDispatcher
    {
        void Register<TState>(IReducer<TState> reducer);
        void Register(ICommandHandler handler);
        TState Reduce<TState>(TState currentState, IIntent intent);
        void Execute(IIntent intent, object state);
    }
}
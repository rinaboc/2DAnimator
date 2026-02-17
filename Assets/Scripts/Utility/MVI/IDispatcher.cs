using System;

namespace Assets.Scripts.Utility.MVI
{
    public interface IDispatcher
    {
        void Register<TState>(IReducer<TState> reducer);
        void Register(ICommandHandler handler);
        void Register(IStore store);
        void Remove(IStore store);
        TState Reduce<TState>(TState currentState, IIntent intent);
        void Execute(IIntent intent, object state);
        void Dispatch(IIntent intent);
        bool TryGetStore<TState>(out Store<TState> store) where TState : IState<TState>;
        void Undo();
        void Redo();
    }
}
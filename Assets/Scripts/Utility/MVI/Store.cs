using System;

namespace Assets.Scripts.Utility.MVI
{
    public interface IStore
    {
        void Reduce(IIntent intent);
        void Execute(IIntent intent);
    }

    public sealed class Store<TState> : IStore
    {
        public TState State { get; private set; }
        public event Action<TState> StateChanged;
        private readonly IDispatcher _dispatcher;

        public Store(TState state, IDispatcher dispatcher)
        {
            State = state;
            _dispatcher = dispatcher;
            _dispatcher.Register(this);
        }

        public void Dispatch(IIntent intent)
        {
            if (IntentHelper.IsGlobalIntent(intent))
                _dispatcher.Dispatch(intent);
            else
                Reduce(intent);

            Execute(intent);
        }

        public void Execute(IIntent intent)
        {
            _dispatcher.Execute(intent, State);
        }


        public void Reduce(IIntent intent)
        {
            var newState = _dispatcher.Reduce(State, intent);
            bool stateChanged = !Equals(State, newState);
            State = newState;

            if (stateChanged)
                StateChanged?.Invoke(State);
        }

        public void Unbind()
        {
            _dispatcher.Remove(this);
        }
    }
}

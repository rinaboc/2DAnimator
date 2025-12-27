using System;

namespace Assets.Scripts.Utility.MVI
{
    public sealed class Store<TState>
    {
        public TState State { get; private set; }
        public event Action<TState> StateChanged;
        private readonly IDispatcher _dispatcher;


        public Store(TState state, IDispatcher dispatcher)
        {
            State = state;
            _dispatcher = dispatcher;
        }

        public void Dispatch(IIntent intent)
        {
            var newState = _dispatcher.Reduce(State, intent);

            bool stateChanged = !Equals(State, newState);
            State = newState;

            _dispatcher.Execute(intent, State);

            if (stateChanged)
                StateChanged?.Invoke(State);

        }
    }
}

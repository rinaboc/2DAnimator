using System;

namespace Assets.Scripts.Utility
{
    public sealed class Store<TState>
    {
        public TState State { get; private set; }
        public event Action<TState> StateChanged;

        private readonly IReducer<TState> _reducer;

        public Store(TState state, IReducer<TState> reducer)
        {
            State = state;
            _reducer = reducer;
        }

        public void Dispatch(IIntent intent)
        {
            State = _reducer.Reduce(State, intent);
            StateChanged?.Invoke(State);
        }
    }
}

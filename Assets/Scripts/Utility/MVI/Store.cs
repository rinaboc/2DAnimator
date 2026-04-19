using System;
using System.Collections.Generic;

namespace Assets.Scripts.Utility.MVI
{
    public interface IStore
    {
        void Reduce(IIntent intent);
        Type GetStateType();
        void SetState(object state);
        object GetState();
        void UpdateState();
    }

    public sealed class Store<TState> : IStore where TState : IState<TState>
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

        public Type GetStateType()
        {
            return typeof(TState);
        }

        public void Dispatch(IIntent intent)
        {
            _dispatcher.Dispatch(intent, this);
        }

        public void Reduce(IIntent intent)
        {
            var newState = _dispatcher.Reduce(State, intent);
            bool stateChanged = !Equals(State, newState);
            State = newState;

            if (!stateChanged) return;

            StateChanged?.Invoke(State);

        }

        public void Unbind()
        {
            _dispatcher.Remove(this);
        }

        public void SetState(object state)
        {
            State = (TState)state;
            StateChanged?.Invoke(State);
        }

        public object GetState()
        {
            return State.Clone();
        }

        public void UpdateState()
        {
            State = _dispatcher.Update(State);
            StateChanged?.Invoke(State);
        }
    }
}

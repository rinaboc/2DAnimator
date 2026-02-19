using System;
using System.Collections.Generic;

namespace Assets.Scripts.Utility.MVI
{
    public interface IStore
    {
        void Reduce(IIntent intent);
        void Execute(IIntent intent);
        Type GetStateType();
        void Undo();
        void Redo();
    }

    public sealed class Store<TState> : IStore where TState : IState<TState>
    {
        public TState State { get; private set; }
        public event Action<TState> StateChanged;
        private readonly IDispatcher _dispatcher;

        readonly BoundedStack<TState> _stateHistory = new(40);
        readonly Stack<KeyValuePair<IIntent, TState>> _futureStates = new();

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
            if (intent is IIntentUndo) { _dispatcher.Undo(); return; }
            else if (intent is IIntentRedo) { _dispatcher.Redo(); return; }
            else if (intent is InitializeProjectIntent)
            {
                _stateHistory.Clear();
                _futureStates.Clear();
            }

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
            _stateHistory.Push(new(intent, State.Clone()));

            var newState = _dispatcher.Reduce(State, intent);
            bool stateChanged = !Equals(State, newState);
            State = newState;

            if (_futureStates.Count > 0)
            {
                _futureStates.Clear();
                _futureStates.TrimExcess();
            }

            if (!stateChanged) return;

            StateChanged?.Invoke(State);

        }

        public void Unbind()
        {
            _dispatcher.Remove(this);
        }

        public void Undo()
        {
            do
            {
                if (!_stateHistory.TryPop(out var prevState)) return;
                var (intent, state) = prevState;
                _futureStates.Push(new(intent, State));
                State = state;
                Execute(intent);
            } while (_stateHistory.Count > 0 && !IntentHelper.IsUndoableIntent(_futureStates.Peek().Key));

            StateChanged?.Invoke(State);
        }

        public void Redo()
        {
            do
            {
                if (!_futureStates.TryPop(out var nextState)) return;
                var (intent, state) = nextState;
                _stateHistory.Push(new(intent, State));
                State = state;
                Execute(intent);
            } while (_futureStates.Count > 0 && !IntentHelper.IsUndoableIntent(_futureStates.Peek().Key));

            StateChanged?.Invoke(State);
        }
    }
}

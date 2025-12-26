using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.Utility
{
    public sealed class Store<TState>
    {
        public TState State { get; private set; }
        public event Action<TState> StateChanged;

        private readonly IReducer<TState> _reducer;
        private readonly IReadOnlyList<ICommandHandler> _handlers;
        private readonly IModelContext _context;

        public Store(TState state, IReducer<TState> reducer, IEnumerable<ICommandHandler> handlers, IModelContext context)
        {
            State = state;
            _reducer = reducer;
            _handlers = handlers.ToList();
            _context = context;
        }

        public void Dispatch(IIntent intent)
        {
            var newState = _reducer.Reduce(State, intent);
            bool stateChanged = !Equals(State, newState);

            State = newState;

            foreach (var handler in _handlers)
            {
                if (handler.CanHandle(intent))
                    handler.Execute(intent, State, _context);
            }

            if (stateChanged)
                StateChanged?.Invoke(State);

        }
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Utility.MVI
{
    public class Dispatcher : IDispatcher
    {
        private readonly Dictionary<Type, List<object>> _typedReducers = new();
        private readonly List<ICommandHandler> _handlers = new();
        private readonly IModelContext _context;
        private readonly List<IStore> _stores = new();


        public Dispatcher(IModelContext context)
        {
            _context = context;
        }

        public void Dispatch(IIntent intent)
        {
            foreach (var store in _stores)
            {
                store.Reduce(intent);
            }
        }

        public void Execute(IIntent intent, object state)
        {
            foreach (var handler in _handlers)
            {
                handler.Execute(intent, state, _context);
            }
        }

        public TState Reduce<TState>(TState currentState, IIntent intent)
        {
            var stateType = typeof(TState);
            if (_typedReducers.TryGetValue(stateType, out var reducers))
            {
                foreach (var reducerObj in reducers)
                {
                    var reducer = (IReducer<TState>)reducerObj;
                    return reducer.Reduce(currentState, intent);
                }
            }
            Debug.Log($"No reducer found for state type {stateType} and intent {intent.GetType()}");
            return currentState;
        }

        public void Register(ICommandHandler handler)
        {
            _handlers.Add(handler);
        }

        public void Register<TState>(IReducer<TState> reducer)
        {
            var stateType = typeof(TState);
            if (!_typedReducers.ContainsKey(stateType))
            {
                _typedReducers[stateType] = new List<object>();
            }
            _typedReducers[stateType].Add(reducer);
        }

        public void Register(IStore store)
        {
            _stores.Add(store);
        }

        public void Remove(IStore store)
        {
            _stores.Remove(store);
        }
    }
}
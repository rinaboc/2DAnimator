using System;
using System.Collections.Generic;
using Assets.Scripts.States;
using UnityEngine;

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
        void Dispatch(IIntent intent, IStore store = null);
        bool TryGetStore<TState>(out Store<TState> store) where TState : IState<TState>;
        void Undo();
        void Redo();
    }

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

        public void Dispatch(IIntent intent, IStore source)
        {
            TryGetStore<OperationState>(out var operationStore);

            switch (intent)
            {
                case UndoIntent: Undo(); operationStore.Reduce(intent); return;
                case RedoIntent: Redo(); operationStore.Reduce(intent); return;
                case InitializeProjectIntent:
                    foreach (var store in _stores)
                    {
                        store.ClearHistory();
                        store.Reduce(intent);
                    }
                    return;
            }


            foreach (var store in _stores)
            {
                if (intent is IIntentUnstored) break;
                if (store == operationStore) continue;

                store.CreateSnapshot(intent);

                store.PrintHistory();
            }

            if (!IntentHelper.IsGlobalIntent(intent) && source != null)
            {
                source.Reduce(intent);
                operationStore.Reduce(intent);
                return;
            }

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

        public bool TryGetStore<TState>(out Store<TState> store) where TState : IState<TState>
        {
            store = null;
            foreach (var s in _stores)
            {
                if (s.GetStateType() == typeof(TState))
                {
                    store = (Store<TState>)s;
                    return true;
                }
            }
            return false;
        }

        public void Undo()
        {
            foreach (var store in _stores)
            {
                store.Undo();
                store.PrintHistory();
            }
        }

        public void Redo()
        {
            foreach (var store in _stores)
            {
                store.Redo();
                store.PrintHistory();
            }
        }
    }
}
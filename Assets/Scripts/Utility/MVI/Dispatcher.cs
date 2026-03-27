using System;
using System.Collections.Generic;
using System.Linq;
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
        TState Update<TState>(TState currentState);
        void Execute(IIntent intent);
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
        private readonly IUndoAPI _undoAPI;

        public Dispatcher(IModelContext context, IUndoAPI undoAPI)
        {
            _context = context;
            _undoAPI = undoAPI;
        }

        public void Dispatch(IIntent intent, IStore source)
        {
            switch (intent)
            {
                case UndoIntent: Undo(); return;
                case RedoIntent: Redo(); return;
            }

            _context.SessionInfo.HistoryReset = true;
            List<Action> actions = new();
            foreach (var handler in _handlers)
            {
                var action = handler.Execute(intent, _context);
                if (action != null) actions.Add(action);
            }

            if (intent is not IIntentUnstored)
                _undoAPI.Save(intent, _stores.Select(s => new KeyValuePair<IStore, object>(s, s.GetState())).ToList(), actions);
            _undoAPI.PrintHistory();

            if (!IntentHelper.IsGlobalIntent(intent) && source != null)
            {
                source.Reduce(intent);
                return;
            }

            foreach (var store in _stores)
            {
                store.Reduce(intent);
            }
        }

        public void Execute(IIntent intent)
        {
            foreach (var handler in _handlers)
            {
                handler.Execute(intent, _context);
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
                    var updatedState = reducer.Update(currentState, _context);
                    return reducer.Reduce(updatedState, intent);
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
            _context.SessionInfo.HistoryReset = false;
            _undoAPI.Undo();
            foreach (var store in _stores) store.UpdateState();
        }

        public void Redo()
        {
            _context.SessionInfo.HistoryReset = false;
            _undoAPI.Redo(this);
            foreach (var store in _stores) store.UpdateState();
        }

        public TState Update<TState>(TState currentState)
        {
            var stateType = typeof(TState);
            if (_typedReducers.TryGetValue(stateType, out var reducers))
            {
                foreach (var reducerObj in reducers)
                {
                    var reducer = (IReducer<TState>)reducerObj;
                    return reducer.Update(currentState, _context);
                }
            }
            return currentState;
        }
    }
}
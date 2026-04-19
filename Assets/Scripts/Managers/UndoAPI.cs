using System;
using System.Collections.Generic;
using Assets.Scripts.Utility;
using Assets.Scripts.Utility.MVI;
using UnityEditorInternal;

public interface IUndoAPI
{
    void Save(IIntent intent, List<KeyValuePair<IStore, object>> states, List<Action> actions);
    void Reset();
    void Undo();
    void Redo(IDispatcher dispatcher);
    void PrintHistory();
}

public class UndoAPI : IUndoAPI
{
    private BoundedStack _appHistory = new(GeneralSettings.Instance.HistoryLimit);
    private Stack<Snapshot> _appFuture = new();

    public void PrintHistory()
    {
        _appHistory.Print();
    }

    public void Reset()
    {
        _appFuture.Clear();
        _appFuture.TrimExcess();
    }

    public void Save(IIntent intent, List<KeyValuePair<IStore, object>> states, List<Action> actions)
    {
        var snapshot = new Snapshot(intent, states);
        foreach (var action in actions) snapshot.AddUndoAction(action);
        _appHistory.Push(snapshot);
        if (_appFuture.Count > 0) Reset();
    }

    public void Undo()
    {
        do
        {
            if (!_appHistory.TryPop(out var prevState)) return;
            _appFuture.Push(prevState.Clone());
            foreach (var state in prevState.States) state.Key.SetState(state.Value);
            foreach (var action in prevState.UndoActions) action();
        } while (_appHistory.Count > 0 &&
            (!IntentHelper.IsUndoableIntent(_appFuture.Peek().ReceivedIntent)));
    }

    public void Redo(IDispatcher dispatcher)
    {
        do
        {
            if (!_appFuture.TryPop(out var nextState)) return;
            _appHistory.Push(nextState.Clone());
            dispatcher.Execute(nextState.ReceivedIntent);
            foreach (var state in nextState.States)
            {
                var currentState = Convert.ChangeType(state.Key.GetState(), state.Key.GetStateType());
                var reduceMethod = dispatcher.GetType().GetMethod("Reduce").MakeGenericMethod(state.Key.GetStateType());
                var reducedState = reduceMethod.Invoke(dispatcher, new object[] { currentState, nextState.ReceivedIntent });
                state.Key.SetState(reducedState);
            }
        } while (_appFuture.Count > 0 &&
            (!IntentHelper.IsUndoableIntent(_appFuture.Peek().ReceivedIntent)));
    }
}

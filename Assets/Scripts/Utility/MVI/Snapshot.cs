using System;
using System.Collections.Generic;

namespace Assets.Scripts.Utility.MVI
{
    public class Snapshot
    {
        public IIntent ReceivedIntent { get; }
        public List<KeyValuePair<IStore, object>> States { get; }
        public List<Action> UndoActions { get; set; }

        public Snapshot(IIntent intent, List<KeyValuePair<IStore, object>> states)
        {
            ReceivedIntent = intent;
            States = states;
            UndoActions = new();
        }

        public void AddUndoAction(Action action)
        {
            UndoActions.Add(action);
        }

        public Snapshot Clone() => new(ReceivedIntent, States)
        {
            UndoActions = UndoActions
        };

    }
}
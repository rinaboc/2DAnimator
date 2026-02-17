using System.Collections.Generic;
using Assets.Scripts.Utility.MVI;

namespace Assets.Scripts.Utility
{
    public class BoundedStack<TState>
    {
        private readonly int _capacity;
        private readonly LinkedList<KeyValuePair<IIntent, TState>> _list = new();

        public int Count { get => _list.Count; }

        public BoundedStack(int capacity)
        {
            _capacity = capacity;
        }

        public void Push(KeyValuePair<IIntent, TState> item)
        {
            if (_list.Count >= _capacity)
            {
                _list.RemoveFirst();
                while (_list.Count > 0 && !IntentHelper.IsUndoableIntent(_list.First.Value.Key))
                    _list.RemoveFirst();
            }

            _list.AddLast(item);
        }

        public bool TryPop(out KeyValuePair<IIntent, TState> element)
        {
            element = default;
            if (_list.Count == 0) return false;

            element = _list.Last.Value;
            _list.RemoveLast();
            return true;
        }

        public void Clear()
        {
            _list.Clear();
        }
    }
}


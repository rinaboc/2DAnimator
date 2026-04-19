using System.Collections.Generic;
using Assets.Scripts.Utility.MVI;
using UnityEngine;

namespace Assets.Scripts.Utility
{
    public class BoundedStack
    {
        private readonly int _capacity;
        private readonly LinkedList<Snapshot> _list = new();

        public int Count { get => _list.Count; }

        public BoundedStack(int capacity)
        {
            _capacity = capacity;
        }

        public void Push(Snapshot item)
        {
            if (_list.Count >= _capacity)
            {
                _list.RemoveFirst();
                while (_list.Count > 0 && !IntentHelper.IsUndoableIntent(_list.First.Value.ReceivedIntent))
                    _list.RemoveFirst();
            }

            _list.AddLast(item);
        }

        public bool TryPop(out Snapshot element)
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

        public Snapshot Peek() => _list.Last.Value;
        public void Print()
        {
            string log = $"App's history: \n";
            var current = _list.Last;
            while (current != null)
            {
                var item = current.Value;
                log += $"intent {item.ReceivedIntent.GetType().Name} \n";
                current = current.Previous;
            }

            Debug.Log(log);
        }
    }
}


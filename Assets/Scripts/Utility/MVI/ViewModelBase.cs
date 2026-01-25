using System.Collections.Generic;
using Assets.Scripts.Utility.MVI;
using UnityEngine;

public class ViewModelBase<TState> : MonoBehaviour, IViewModel<TState>
{
    protected Store<TState> _store;
    protected List<IView<TState>> _views = new();

    public void Bind(Store<TState> store)
    {
        _store = store;
        _store.StateChanged += OnStateChanged;
    }

    public void Bind(IView<TState> view)
    {
        _views.Add(view);
        view.Render(_store.State);
    }

    public void Unbind(IView<TState> view)
    {
        _views.Remove(view);
    }

    public void Unbind()
    {
        _store.StateChanged -= OnStateChanged;
    }

    public void Send(IIntent intent)
    {
        _store.Dispatch(intent);
    }

    protected void OnStateChanged(TState state)
    {
        var views = new List<IView<TState>>(_views);
        foreach (var view in views)
        {
            if (_views.Contains(view))
                view.Render(state);
        }
    }

    void OnDestroy()
    {
        Unbind();
        _store.Unbind();
    }
}

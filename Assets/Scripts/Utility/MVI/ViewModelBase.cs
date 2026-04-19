using System.Collections.Generic;
using Assets.Scripts.Utility.MVI;
using UnityEngine;

public abstract class ViewModelBase<TDomainState, TViewState> : MonoBehaviour, IViewModel<TDomainState, TViewState>
where TDomainState : IState<TDomainState>
{
    protected Store<TDomainState> _store;
    protected List<IView<TDomainState, TViewState>> _views = new();

    protected abstract TViewState Project(TDomainState domain);

    public void Bind(Store<TDomainState> store)
    {
        _store = store;
        _store.StateChanged += OnStateChanged;
    }

    public void Bind(IView<TDomainState, TViewState> view)
    {
        _views.Add(view);
        view.Render(Project(_store.State));
    }

    public void Unbind(IView<TDomainState, TViewState> view)
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

    protected void OnStateChanged(TDomainState state)
    {
        var views = new List<IView<TDomainState, TViewState>>(_views);
        foreach (var view in views)
        {
            if (_views.Contains(view))
                view.Render(Project(state));
        }
    }

    void OnDestroy()
    {
        Unbind();
        _store.Unbind();
    }
}

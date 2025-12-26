using System.Collections.Generic;
using Assets.Scripts.Utility;
using UnityEngine;

public sealed class ParametersViewModel : MonoBehaviour, IViewModel<ParameterStates>
{
    private Store<ParameterStates> _store;
    private List<IView<ParameterStates>> _views = new();

    public void Bind(Store<ParameterStates> store)
    {
        _store = store;
        _store.StateChanged += OnStateChanged;
    }

    public void Bind(IView<ParameterStates> view)
    {
        _views.Add(view);
        view.Render(_store.State);
    }

    public void Unbind(IView<ParameterStates> view)
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

    private void OnStateChanged(ParameterStates state)
    {
        foreach (var view in _views)
        {
            view.Render(state);
        }
    }

    void OnDestroy()
    {
        Unbind();
    }
}


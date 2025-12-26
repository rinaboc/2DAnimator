using System.Collections.Generic;
using Assets.Scripts.ArtMesh;
using Assets.Scripts.Utility;
using UnityEngine;

public sealed class MeshViewModel : MonoBehaviour, IViewModel<MeshState>
{
    private Store<MeshState> _store;
    private List<IView<MeshState>> _views = new();

    public void Bind(Store<MeshState> store)
    {
        _store = store;
        _store.StateChanged += OnStateChanged;
    }

    public void Bind(IView<MeshState> view)
    {
        _views.Add(view);
        view.Render(_store.State);
    }

    public void Unbind(IView<MeshState> view)
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

    private void OnStateChanged(MeshState state)
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


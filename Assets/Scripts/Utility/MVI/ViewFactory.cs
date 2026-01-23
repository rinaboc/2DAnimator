using System;
using Assets.Scripts.Utility.MVI;
using UnityEngine;

public class ViewFactory : MonoBehaviour
{
    public static ViewFactory Instance { get; private set; }

    public void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    [SerializeField] private AppInitializer _appInitializer;

    public TView CreateView<TView, TState>()
        where TView : IView<TState>, new()
        where TState : class
    {
        var view = new TView();
        _appInitializer.GetViewModel(out IViewModel<TState> viewModel);
        view.SetViewModel(viewModel);
        return view;
    }

    public TView CreateView<TView, TState>(params object[] constructorArgs)
        where TView : IView<TState>
        where TState : class
    {
        var view = (TView)Activator.CreateInstance(typeof(TView), constructorArgs);
        _appInitializer.GetViewModel(out IViewModel<TState> viewModel);
        view.SetViewModel(viewModel);
        return view;
    }
}

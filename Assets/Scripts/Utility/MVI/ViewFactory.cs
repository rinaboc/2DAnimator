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

    public TView CreateView<TView, TDomain, TViewState>()
        where TView : IView<TDomain, TViewState>, new()
        where TDomain : IState<TDomain>
    {
        var view = new TView();
        _appInitializer.GetViewModel(out IViewModel<TDomain, TViewState> viewModel);
        view.SetViewModel(viewModel);
        return view;
    }

    public TView CreateView<TView, TDomain, TViewState>(params object[] constructorArgs)
        where TView : IView<TDomain, TViewState>
        where TDomain : IState<TDomain>
    {
        var view = (TView)Activator.CreateInstance(typeof(TView), constructorArgs);
        _appInitializer.GetViewModel(out IViewModel<TDomain, TViewState> viewModel);
        view.SetViewModel(viewModel);
        return view;
    }
}

using Assets.Scripts.Utility.MVI;
using UnityEngine;

public class ViewModelFactory : MonoBehaviour
{
    public static ViewModelFactory Instance { get; private set; }

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

    public IViewModel<TDomain, TView> CreateViewModel<TViewModel, TDomain, TView>(GameObject parent)
    where TViewModel : MonoBehaviour, IViewModel<TDomain, TView>
    where TDomain : new()
    {
        var state = new TDomain();
        var store = new Store<TDomain>(state, _appInitializer.Dispatcher);
        var viewModel = parent.AddComponent<TViewModel>();
        viewModel.Bind(store);
        return viewModel;
    }

    public IViewModel<TDomain, TView> CreateViewModel<TViewModel, TDomain, TView>(GameObject parent, AppInitializer appInitializer)
    where TViewModel : MonoBehaviour, IViewModel<TDomain, TView>
    where TDomain : new()
    {
        var state = new TDomain();
        var store = new Store<TDomain>(state, appInitializer.Dispatcher);
        var viewModel = parent.AddComponent<TViewModel>();
        viewModel.Bind(store);
        return viewModel;
    }

    public IViewModel<TDomain, TView> CreateViewModel<TViewModel, TDomain, TView>(GameObject parent, TDomain state)
    where TViewModel : MonoBehaviour, IViewModel<TDomain, TView>
    where TDomain : new()
    {
        var store = new Store<TDomain>(state, _appInitializer.Dispatcher);
        var viewModel = parent.AddComponent<TViewModel>();
        viewModel.Bind(store);
        return viewModel;
    }

    public IViewModel<TDomain, TView> CreateSharedViewModel<TViewModel, TDomain, TView>(GameObject parent)
    where TViewModel : MonoBehaviour, IViewModel<TDomain, TView>
    where TDomain : new()
    {
        if (!_appInitializer.Dispatcher.TryGetStore<TDomain>(out var store)) { Debug.LogError("Couldn't fetch store"); return null; }
        var viewModel = parent.AddComponent<TViewModel>();
        viewModel.Bind(store);
        return viewModel;
    }
}

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

    public IViewModel<TState> CreateViewModel<TViewModel, TState>(GameObject parent)
    where TViewModel : MonoBehaviour, IViewModel<TState>
    where TState : new()
    {
        var state = new TState();
        var store = new Store<TState>(state, _appInitializer.Dispatcher);
        var viewModel = parent.AddComponent<TViewModel>();
        viewModel.Bind(store);
        return viewModel;
    }

    public IViewModel<TState> CreateViewModel<TViewModel, TState>(GameObject parent, TState state)
    where TViewModel : MonoBehaviour, IViewModel<TState>
    where TState : new()
    {
        var store = new Store<TState>(state, _appInitializer.Dispatcher);
        var viewModel = parent.AddComponent<TViewModel>();
        viewModel.Bind(store);
        return viewModel;
    }
}

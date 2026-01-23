using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.States;
using Assets.Scripts.Utility.MVI;
using UnityEngine;

public class AppInitializer : MonoBehaviour
{
    [SerializeField] private MeshRegistry _meshRegistry;
    [SerializeField] private ParameterRegistry _parameterRegistry;
    [SerializeField] private ParamCurveRegistry _paramCurveRegistry;
    [SerializeField] private ParamPointRegistry _paramPointRegistry;
    [SerializeField] private KeyFrameRegistry _keyFrameRegistry;
    [SerializeField] private GeneralSettings _generalSettings;

    public IModelContext Context { get => _context; }
    private IModelContext _context;

    public IDispatcher Dispatcher { get => _dispatcher; }
    private IDispatcher _dispatcher;

    private readonly Dictionary<Type, List<object>> _typedViewModels = new();


    [SerializeField] private NFPController _nfpController;
    [SerializeField] private TimelineWidgetController _timelineWidgetController;
    [SerializeField] private TimelineSettingsController _timelineSettingsController;
    [SerializeField] private ParameterSettingsView _parameterSettingsView;

    void Awake()
    {
        _context = new AppModelContext(
            _meshRegistry,
            _parameterRegistry,
            _paramCurveRegistry,
            _paramPointRegistry,
            _keyFrameRegistry,
            _generalSettings
        );

        _dispatcher = new Dispatcher(_context);

        _dispatcher.Register(new MeshCommandHandler());
        _dispatcher.Register(new ParameterCommandHandler());
        _dispatcher.Register(new LayerCommandHandler());
        _dispatcher.Register(new TimelineCommandHandler());

        _dispatcher.Register(new MeshReducer());
        _dispatcher.Register(new ParameterReducer());
        _dispatcher.Register(new LayerReducer());
        _dispatcher.Register(new TimelineReducer());
    }

    void Start()
    {
        CreateViewModels();
        BindViews();
    }

    private void BindViews()
    {
        if (GetViewModel<OperationState>(out var operationViewModel))
        {
            _nfpController.SetViewModel(operationViewModel);
        }

        if (GetViewModel<TimelineState>(out var timelineViewModel))
        {
            _timelineWidgetController.SetViewModel(timelineViewModel);
            _timelineSettingsController.SetViewModel(timelineViewModel);
        }

        if (GetViewModel<ParameterStates>(out var parameterViewModel))
        {
            _parameterSettingsView.SetViewModel(parameterViewModel);
        }
    }

    private void CreateViewModels()
    {
        var operationViewModel = ViewModelFactory.Instance.CreateViewModel<OperationViewModel, OperationState>(gameObject);
        Register(operationViewModel);

        var timelineViewModel = ViewModelFactory.Instance.CreateViewModel<TimelineViewModel, TimelineState>(gameObject);
        Register(timelineViewModel);

        var parameterViewModel = ViewModelFactory.Instance.CreateViewModel<ParametersViewModel, ParameterStates>(gameObject);
        Register(parameterViewModel);

        var meshViewModel = ViewModelFactory.Instance.CreateViewModel<MeshViewModel, MeshStates>(gameObject);
        Register(meshViewModel);
    }

    private void Register<TState>(IViewModel<TState> viewModel)
    {
        var stateType = typeof(TState);
        if (!_typedViewModels.ContainsKey(stateType))
        {
            _typedViewModels[stateType] = new List<object>();
        }
        _typedViewModels[stateType].Add(viewModel);
    }

    public bool GetViewModel<TState>(out IViewModel<TState> viewModel) where TState : class
    {
        viewModel = null;
        var stateType = typeof(TState);
        if (_typedViewModels.TryGetValue(stateType, out var viewModels))
        {
            foreach (var viewModelObj in viewModels)
            {
                viewModel = (IViewModel<TState>)viewModelObj;
                return true;
            }
        }
        return false;
    }
}

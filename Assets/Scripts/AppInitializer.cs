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

        _dispatcher.Register(new OperationCommandHandler());
        _dispatcher.Register(new ParameterCommandHandler());
        _dispatcher.Register(new TimelineCommandHandler());
        _dispatcher.Register(new MeshLayerCommandHandler());

        _dispatcher.Register(new MeshLayerReducer());
        _dispatcher.Register(new ParameterTimelineReducer(new ParameterReducer(), new TimelineReducer()));
    }

    void Start()
    {
        CreateViewModels();
        BindViews();
    }

    private void BindViews()
    {
        if (GetViewModel<ParameterTimelineState, TimelineState>(out var timelineViewModel))
        {
            _timelineWidgetController.SetViewModel(timelineViewModel);
            _timelineSettingsController.SetViewModel(timelineViewModel);
        }

        if (GetViewModel<ParameterTimelineState, ParameterStates>(out var parameterViewModel))
        {
            _parameterSettingsView.SetViewModel(parameterViewModel);
        }
    }

    private void CreateViewModels()
    {
        var operationViewModel = ViewModelFactory.Instance.CreateViewModel<OperationViewModel, OperationState, OperationState>(gameObject);
        Register(operationViewModel);

        var timelineViewModel = ViewModelFactory.Instance.CreateViewModel<TimelineViewModel, ParameterTimelineState, TimelineState>(gameObject);
        Register(timelineViewModel);

        var parameterViewModel = ViewModelFactory.Instance.CreateSharedViewModel<ParametersViewModel, ParameterTimelineState, ParameterStates>(gameObject);
        Register(parameterViewModel);

        var meshViewModel = ViewModelFactory.Instance.CreateViewModel<MeshViewModel, MeshLayerStates, MeshStates>(gameObject);
        Register(meshViewModel);

        var layerViewModel = ViewModelFactory.Instance.CreateSharedViewModel<LayersViewModel, MeshLayerStates, LayerStates>(gameObject);
        Register(layerViewModel);

    }

    private void Register<TDomain, TView>(IViewModel<TDomain, TView> viewModel)
    {
        var stateType = typeof(TDomain);
        if (!_typedViewModels.ContainsKey(stateType))
        {
            _typedViewModels[stateType] = new List<object>();
        }
        _typedViewModels[stateType].Add(viewModel);
    }

    public bool GetViewModel<TDomain, TView>(out IViewModel<TDomain, TView> viewModel) where TDomain : class
    {
        viewModel = null;
        var stateType = typeof(TDomain);
        if (_typedViewModels.TryGetValue(stateType, out var viewModels))
        {
            foreach (var viewModelObj in viewModels)
            {
                var viewType = typeof(IViewModel<TDomain, TView>);
                if (viewType.IsAssignableFrom(viewModelObj.GetType()))
                {
                    viewModel = (IViewModel<TDomain, TView>)viewModelObj;
                    return true;
                }
            }
        }
        return false;
    }
}

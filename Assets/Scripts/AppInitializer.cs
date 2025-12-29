using System.Collections.Generic;
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

    [SerializeField] private NFPController _nfpController;


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

        _dispatcher.Register(new MeshReducer());
        _dispatcher.Register(new ParameterReducer());
        _dispatcher.Register(new LayerReducer());
    }

    void Start()
    {
        CreateViewModels();
    }

    private void CreateViewModels()
    {
        var operationViewModel = ViewModelFactory.Instance.CreateViewModel<OperationViewModel, OperationState>(gameObject);
        _nfpController.SetViewModel(operationViewModel);
    }
}

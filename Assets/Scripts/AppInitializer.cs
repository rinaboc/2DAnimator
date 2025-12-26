using Assets.Scripts.Utility;
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
    }
}

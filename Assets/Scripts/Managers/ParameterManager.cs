using System;
using System.Collections.Generic;
using Assets.Scripts.Utility;
using UnityEngine;

public class ParameterManager : ManagerBase<ParameterManager>
{
    [SerializeField] private GameObject ParamSliderPrefab;
    [SerializeField] private Transform ParamWidgetContent;

    private readonly Dictionary<Guid, GameObject> _paramSliders = new();
    private ParameterReducer _reducer = new();
    private Store<ParameterStates> _store;
    private ParameterStates _state;
    private ParametersViewModel _viewModel;


    [SerializeField] private AppInitializer _appInitializer;
    private IModelContext _context;
    private ICommandHandler _commandHandler;

    public ParameterSlider GetParamSlider(Guid id) => _paramSliders[id].GetComponent<ParameterSlider>();

    protected override void Awake()
    {
        base.Awake();
        _context = _appInitializer.Context;
        _commandHandler = new ParameterCommandHandler();
        _state = new ParameterStates()
        {
            Parameters = new Dictionary<Guid, ParameterStates.ParameterState>()
        };
        _store = new Store<ParameterStates>(_state, _reducer, new ICommandHandler[] { _commandHandler }, _context);
        _viewModel = gameObject.AddComponent<ParametersViewModel>();
        _viewModel.Bind(_store);
    }

    void Start()
    {
        CreateDebugParam();
    }

    void OnEnable()
    {
        UIEvents.LayerDeleteEvent += DeleteParamPointsOfMesh;
        UIEvents.LayerSelectEvent += OnLayerSelect;
    }

    void OnDisable()
    {
        UIEvents.LayerDeleteEvent -= DeleteParamPointsOfMesh;
        UIEvents.LayerSelectEvent -= OnLayerSelect;
    }

    public bool ParameterWidgetVisibility
    {
        set
        {
            ParamWidgetContent.gameObject.SetActive(value);
        }
    }

    public void CreateDebugParam()
    {
        CreateParameter(0, 1, 0);
    }

    public void CreatePointsForCurrentMesh()
    {
        MeshController selectedArtMesh = LayerManager.Instance.SelectedArtMesh;
        if (selectedArtMesh == null) return;

        _store.Dispatch(new CreateParamPointsIntent(selectedArtMesh.ID));
    }

    /// <summary>
    /// Called when there's an update made to a parameter data object. Updates parameter slider.
    /// </summary>
    public void UpdateParameter(Parameter parameter)
    {
        ParameterSlider parameterSlider = GetParamSlider(parameter.ID);
        parameterSlider.UpdateSlider(parameter);
    }

    /// <summary>
    /// Call popup window to edit parameter details.
    /// </summary>
    public void StartParameterEditing(Guid paramID)
    {
        UIEvents.RaiseEditParameterInfo(paramID);
    }

    public void CreateParameter(float min, float max, float defaultValue, string name = "parameter")
    {
        _store.Dispatch(new CreateParameterIntent(Guid.NewGuid(), min, max, defaultValue, name));
    }

    public void CreateParameterSlider(Parameter parameter)
    {
        GameObject paramSlider = Instantiate(ParamSliderPrefab, ParamWidgetContent);
        ParameterSlider parameterSlider = paramSlider.GetComponent<ParameterSlider>();
        parameterSlider.SetParamID(parameter.ID);

        _paramSliders.Add(parameter.ID, paramSlider);
        parameterSlider.SetViewModel(_viewModel);

    }

    public void DeleteParameterSlider(Guid id)
    {
        if (_paramSliders.TryGetValue(id, out GameObject paramSlider))
        {
            _paramSliders.Remove(id);
            Destroy(paramSlider);
        }
    }

    private void ClearParamSliders()
    {
        foreach (var item in _paramSliders)
        {
            Destroy(item.Value);
        }
        _paramSliders.Clear();
        ParameterRegistry.Instance.Clear();
    }


    public void DeleteSelectedParameterSlider()
    {
        _store.Dispatch(new DeleteSelectedParameterIntent());
    }

    public void CreateParamPoints(Guid parameterID, Guid meshID)
    {
        ParameterRegistry.Instance.TryGet(parameterID, out Parameter parameter);
        ParamCurve paramCurve = new(meshID, parameter.ID);
        ParamPoint minPoint = new(parameter.MinValue);
        ParamPoint maxPoint = new(parameter.MaxValue);

        paramCurve.ParamPoints.Add(minPoint.ID);
        paramCurve.ParamPoints.Add(maxPoint.ID);

        parameter.ParamCurves.Add(paramCurve.ID);

        List<float> paramValues = new()
        {
            minPoint.ParamValue,
            maxPoint.ParamValue
        };

        if (Math.Abs(parameter.MinValue - parameter.DefaultValue) > 0.1f
        && Math.Abs(parameter.MaxValue - parameter.DefaultValue) > 0.1f)
        {
            ParamPoint midPoint = new(parameter.DefaultValue);
            paramCurve.ParamPoints.Add(midPoint.ID);
            paramValues.Add(midPoint.ParamValue);
        }

        GetParamSlider(parameterID).CreateParamPointHandles(paramValues);

        Debug.Log("created parampoints");
    }

    public void DeleteParamPointsOfMesh(Guid meshID)
    {
        DeleteAnimationDataOfMesh(meshID);
        HighlightCreatedCurves(meshID);
    }

    public void DeleteAnimationDataOfMesh(Guid id)
    {
        ParamCurveRegistry paramCurveRegistry = ParamCurveRegistry.Instance;
        ParamPointRegistry paramPointRegistry = ParamPointRegistry.Instance;
        List<ParamCurve> paramCurves = paramCurveRegistry.GetParamCurvesOfMesh(id);
        for (int i = 0; i < paramCurves.Count; i++)
        {
            ParamCurve paramCurve = paramCurves[i];
            foreach (Guid pointID in paramCurve.ParamPoints)
            {
                paramPointRegistry.Remove(pointID);
            }
            paramCurveRegistry.Remove(paramCurve.ID);
        }
    }

    private void OnLayerSelect(Guid id)
    {
        HighlightCreatedCurves(id);
    }

    public void HighlightCreatedCurves(Guid meshID)
    {
        List<Guid> assignedParams = ParamCurveRegistry.Instance.GetAssignedParamIDsOfMesh(meshID);

        foreach (var item in _paramSliders)
        {
            item.Value.GetComponent<ParameterSlider>().SetAssignedCurve(assignedParams.Contains(item.Key));
        }
    }

    public void DispatchToParameterStore(Guid paramID, IIntent intent)
    {
        _store.Dispatch(intent);
    }

    public override void LoadState(SaveData saveData)
    {
        ParameterRegistry.Instance.Clear();
        ParamCurveRegistry.Instance.Clear();
        ParamPointRegistry.Instance.Clear();
        ClearParamSliders();

        foreach (var item in saveData.Parameters)
        {
            ParameterRegistry.Instance.Register(item);
            CreateParameterSlider(item);
        }

        foreach (var item in saveData.ParamPoints)
        {
            ParamPointRegistry.Instance.Register(item);
        }

        foreach (var item in saveData.ParamCurves)
        {
            ParamCurveRegistry.Instance.Register(item);
            List<float> paramValues = new();
            foreach (var pointID in item.ParamPoints)
            {
                if (ParamPointRegistry.Instance.TryGet(pointID, out ParamPoint point))
                {
                    paramValues.Add(point.ParamValue);
                }
            }
            GetParamSlider(item.ParamID).CreateParamPointHandles(paramValues);
        }

    }
}

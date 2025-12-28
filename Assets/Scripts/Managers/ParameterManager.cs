using System;
using System.Collections.Generic;
using Assets.Scripts.Utility.MVI;
using UnityEngine;

public class ParameterManager : ManagerBase<ParameterManager>
{
    [SerializeField] private GameObject ParamSliderPrefab;
    [SerializeField] private Transform ParamWidgetContent;

    private readonly Dictionary<Guid, GameObject> _paramSliders = new();
    private Store<ParameterStates> _store;
    private ParameterStates _state;
    private ParametersViewModel _viewModel;

    [SerializeField] private ParameterSettingsView _parameterSettingsView;

    [SerializeField] private AppInitializer _appInitializer;

    public ParameterSlider GetParamSlider(Guid id) => _paramSliders[id].GetComponent<ParameterSlider>();

    protected override void Awake()
    {
        base.Awake();
        _state = new ParameterStates()
        {
            Parameters = new Dictionary<Guid, ParameterState>()
        };
        _store = new Store<ParameterStates>(_state, _appInitializer.Dispatcher);
        _viewModel = gameObject.AddComponent<ParametersViewModel>();
        _viewModel.Bind(_store);
    }

    void Start()
    {
        _parameterSettingsView.SetViewModel(_viewModel);
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
        _store.Dispatch(new CreateParameterIntent(Guid.NewGuid(), 0, 1, 0, "parameter"));
    }

    public void CreatePointsForCurrentMesh()
    {
        MeshController selectedArtMesh = LayerManager.Instance.SelectedArtMesh;
        if (selectedArtMesh == null) return;

        _store.Dispatch(new CreateParamPointsIntent(selectedArtMesh.ID));
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

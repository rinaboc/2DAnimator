using System;
using System.Collections.Generic;
using Assets.Scripts.States;
using Assets.Scripts.Utility.MVI;
using UnityEngine;

public class ParameterManager : ManagerBase<ParameterManager>
{
    [SerializeField] private GameObject ParamSliderPrefab;
    [SerializeField] private Transform ParamWidgetContent;

    private readonly Dictionary<Guid, GameObject> _paramSliders = new();
    private IViewModel<ParameterStates> _viewModel;
    [SerializeField] private AppInitializer _appInitializer;

    public ParameterSlider GetParamSlider(Guid id) => _paramSliders[id].GetComponent<ParameterSlider>();

    void Start()
    {
        if (!_appInitializer.GetViewModel(out _viewModel))
        {
            Debug.LogError("Couldn't fetch viewModel");
        }
        CreateDebugParam();
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
        _viewModel?.Send(new CreateParameterIntent(Guid.NewGuid(), 0, 1, 0, "parameter"));
    }

    public void CreatePointsForCurrentMesh()
    {
        _viewModel?.Send(new CreateParamPointsIntent());
    }

    public void CreateParameterSlider(Guid ID)
    {
        GameObject paramSlider = Instantiate(ParamSliderPrefab, ParamWidgetContent);
        ParameterSlider parameterSlider = paramSlider.GetComponent<ParameterSlider>();
        parameterSlider.SetParamID(ID);

        _paramSliders.Add(ID, paramSlider);
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
        _viewModel?.Send(new DeleteSelectedParameterIntent());
    }

    public void HighlightCurves(List<Guid> paramIDs)
    {
        foreach (var item in _paramSliders)
        {
            item.Value.GetComponent<ParameterSlider>().SetAssignedCurve(paramIDs.Contains(item.Key));
        }
    }

    public void DispatchToParameterStore(IIntent intent)
    {
        _viewModel?.Send(intent);
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
            CreateParameterSlider(item.ID);
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

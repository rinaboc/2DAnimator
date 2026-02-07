using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.States;
using Assets.Scripts.Utility.MVI;
using UnityEngine;

public class ParameterManager : ManagerBase<ParameterManager>
{
    [SerializeField] private GameObject ParamSliderPrefab;
    [SerializeField] private Transform ParamWidgetContent;

    private readonly Dictionary<Guid, GameObject> _paramSliders = new();
    private IViewModel<ParameterTimelineState, ParameterStates> _viewModel;
    [SerializeField] private AppInitializer _appInitializer;

    public ParameterSlider GetParamSlider(Guid id) => _paramSliders[id].GetComponent<ParameterSlider>();
    public List<Guid> GetParamSliderIDs() => _paramSliders.Keys.ToList();


    void Start()
    {
        if (!_appInitializer.GetViewModel(out _viewModel))
        {
            Debug.LogError("Couldn't fetch viewModel");
        }
        CreateDebugParam();
    }

    private bool _parameterWidgetVisibility = true;
    public bool ParameterWidgetVisibility
    {
        get => _parameterWidgetVisibility;
        set
        {
            ParamWidgetContent.gameObject.SetActive(value);
            _parameterWidgetVisibility = value;
        }
    }

    public void CreateDebugParam()
    {
        _viewModel?.Send(new CreateParameterIntent(Guid.NewGuid(), -1, 1, 0, "parameter"));
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

    public void ClearParamSliders()
    {
        foreach (var item in _paramSliders)
        {
            Destroy(item.Value);
        }
        _paramSliders.Clear();
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
}

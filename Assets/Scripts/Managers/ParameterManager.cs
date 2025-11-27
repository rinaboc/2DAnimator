using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ParameterManager : ManagerBase<ParameterManager>
{
    [SerializeField] private GameObject ParamSliderPrefab;
    [SerializeField] private Transform ParamWidgetContent;

    private readonly Dictionary<Guid, GameObject> _paramSliders = new();

    private ParameterSlider GetParamSlider(Guid id) => _paramSliders[id].GetComponent<ParameterSlider>();

    private bool _isParamSelected = false;
    private Guid _selectedParamID;
    public Guid SelectedParamID
    {
        get
        {
            return _isParamSelected ? _selectedParamID : throw new Exception("No parameters are selected");
        }
    }

    protected override void Awake()
    {
        base.Awake();
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
        if (_isParamSelected && selectedArtMesh != null)
        {
            CreateParamPoints(SelectedParamID, selectedArtMesh.ID);
            HighlightCreatedCurves(selectedArtMesh.ID);
        }
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
        Parameter parameter = new(min, max, defaultValue, name);
        GameObject paramSlider = Instantiate(ParamSliderPrefab, ParamWidgetContent);
        ParameterSlider parameterSlider = paramSlider.GetComponent<ParameterSlider>();
        parameterSlider.SetParamID(parameter.ID);
        parameterSlider.UpdateSlider(parameter);

        _paramSliders.Add(parameter.ID, paramSlider);
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

    public void SelectParameter(Guid id)
    {
        UIEvents.RaiseParameterSelect(id);
        _selectedParamID = id;
        _isParamSelected = true;
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

    public void UpdateAnimationData(object data, TransformType transformType, Guid meshID)
    {
        if (!_isParamSelected) return;

        ParameterRegistry parameterRegistry = ParameterRegistry.Instance;
        ParamPointRegistry paramPointRegistry = ParamPointRegistry.Instance;

        parameterRegistry.TryGet(SelectedParamID, out Parameter currentParam);
        List<ParamCurve> currentParamCurves = ParamCurveRegistry.Instance.GetEntries(currentParam.ParamCurves);

        float sliderValue = GetParamSlider(currentParam.ID).GetValue();
        for (int i = 0; i < currentParamCurves.Count; i++)
        {
            ParamCurve paramCurve = currentParamCurves[i];

            if (paramCurve.MeshID != meshID) // filter by mesh id
                continue;

            List<ParamPoint> paramPoints = paramPointRegistry.GetEntries(paramCurve.ParamPoints);

            bool isPointUpdated = false;
            float[] distFromPointValues = new float[paramPoints.Count];
            for (int j = 0; j < paramPoints.Count; j++)
            {
                ParamPoint point = paramPoints[j];
                distFromPointValues[j] = point.Dist(sliderValue);

                if (distFromPointValues[j] > 0.01f)
                {
                    continue;
                }

                isPointUpdated = true;
                switch (transformType)
                {
                    case TransformType.POSITION:
                        point.transform.Position = (Vector3)data;
                        break;
                    case TransformType.ROTATION:
                        point.transform.Rotation = (Quaternion)data;
                        break;
                    case TransformType.SCALE:
                        point.transform.Scale = (Vector3)data;
                        break;
                }

                Debug.Log($"updated point at {sliderValue}: " + point);
                break;
            }
            if (!isPointUpdated)
            {
                Debug.Log("no point was updated");
                int minIndex = Array.IndexOf(distFromPointValues, distFromPointValues.Min());
                GetParamSlider(paramCurve.ParamID).SetValue(paramPoints[minIndex].ParamValue);
                AnimationManager.Instance.InterpolateParameter(paramPoints[minIndex].ParamValue, paramCurve.ParamID);
            }
        }

    }

    public override void LoadState(SaveData saveData)
    {
        ParameterRegistry.Instance.Clear();
        ParamCurveRegistry.Instance.Clear();
        ParamPointRegistry.Instance.Clear();

        // TODO: load parameters and parameter points from save data
    }
}

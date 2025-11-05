using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ParameterManager : MonoBehaviour
{
    public static ParameterManager instance;

    [SerializeField] private PopupWindowController popupWindow;

    [SerializeField] private GameObject ParamSliderPrefab;
    [SerializeField] private Transform ParamWidgetContent;

    private readonly Dictionary<Guid, GameObject> paramSliders = new();

    private ParameterSlider GetParamSlider(Guid id) => paramSliders[id].GetComponent<ParameterSlider>();

    private bool _isParamSelected = false;
    private Guid _selectedParamID;
    public Guid SelectedParamID
    {
        get
        {
            return _isParamSelected ? _selectedParamID : throw new Exception("No parameters are selected");
        }
    }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(this);
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
        CreateParameter(0, 1, 0);
    }

    public void CreatePointsForCurrentMesh()
    {
        ArtMesh selectedArtMesh = LayerManager.instance.SelectedArtMesh;
        if (_isParamSelected && selectedArtMesh != null)
        {
            CreateParamPoints(SelectedParamID, selectedArtMesh.MeshID);
            HighlightCreatedCurves(selectedArtMesh.MeshID);
        }
    }

    /// <summary>
    /// Called when there's an update made to a parameter data object. Updates parameter slider.
    /// </summary>
    public void UpdateParameter(Parameter parameter)
    {
        ParameterSlider parameterSlider = paramSliders[parameter.ID].GetComponent<ParameterSlider>();
        parameterSlider.UpdateSlider(parameter);
    }

    /// <summary>
    /// Call popup window to edit parameter details.
    /// </summary>
    public void StartParameterEditing(Guid paramID)
    {
        Parameter parameter = ParameterRegistry.Instance.GetParameter(paramID);
        popupWindow.EditParameter(parameter);
    }

    public void CreateParameter(float min, float max, float defaultValue, string name = "parameter")
    {
        Parameter parameter = new(min, max, defaultValue, name);
        GameObject paramSlider = Instantiate(ParamSliderPrefab, ParamWidgetContent);
        ParameterSlider parameterSlider = paramSlider.GetComponent<ParameterSlider>();
        parameterSlider.SetParamID(parameter.ID);
        parameterSlider.UpdateSlider(parameter);

        paramSliders.Add(parameter.ID, paramSlider);
    }

    public void CreateParamPoints(Guid parameterID, Guid meshID)
    {
        Parameter parameter = ParameterRegistry.Instance.GetParameter(parameterID);
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

        // paramSliders[parameterID].GetComponent<ParameterSlider>()
        GetParamSlider(parameterID).CreateParamPointHandles(paramValues);

        Debug.Log("created parampoints");
    }

    public void DeleteParamPointsOfMesh(Guid meshID)
    {
        ParameterRegistry.Instance.DeleteAnimationDataOfMesh(meshID);
        HighlightCreatedCurves(meshID);
    }

    public void SelectParameter(Guid id)
    {
        if (_isParamSelected)
        {
            GetParamSlider(SelectedParamID).SetSelected(false);
        }

        GetParamSlider(id).SetSelected(true);
        _selectedParamID = id;
        _isParamSelected = true;
    }

    public void HighlightCreatedCurves(Guid meshID)
    {
        List<Guid> assignedParams = ParameterRegistry.Instance.GetAssignedParamIDsOfMesh(meshID);

        foreach (var item in paramSliders)
        {
            item.Value.GetComponent<ParameterSlider>().SetAssignedCurve(assignedParams.Contains(item.Key));
        }
    }

    public void UpdateAnimationData(object data, TransformType transformType, Guid meshID)
    {
        if (!_isParamSelected) return;

        ParameterRegistry parameterRegistry = ParameterRegistry.Instance;

        Parameter currentParam = parameterRegistry.GetParameter(SelectedParamID);
        List<ParamCurve> currentParamCurves = parameterRegistry.GetParamCurve(currentParam.ParamCurves);

        float sliderValue = paramSliders[currentParam.ID].GetComponent<ParameterSlider>().GetValue();
        for (int i = 0; i < currentParamCurves.Count; i++)
        {
            ParamCurve paramCurve = currentParamCurves[i];

            if (paramCurve.MeshID != meshID) // filter by mesh id
                continue;

            List<ParamPoint> paramPoints = parameterRegistry.GetParamPoint(paramCurve.ParamPoints);

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
                        point.Position = (Vector3)data;
                        break;
                    case TransformType.ROTATION:
                        point.Rotation = (Quaternion)data;
                        break;
                    case TransformType.SCALE:
                        point.Scale = (Vector3)data;
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
                AnimationManager.instance.InterpolateParameter(paramPoints[minIndex].ParamValue, paramCurve.ParamID);
            }
        }

    }
}

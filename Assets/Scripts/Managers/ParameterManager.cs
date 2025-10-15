using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ParameterManager : MonoBehaviour
{
    public static ParameterManager instance;

    [SerializeField] private GameObject ParamSliderPrefab;
    [SerializeField] private Transform ParamWidgetContent;

    private readonly Dictionary<ushort, GameObject> paramSliders = new();

    private int _selectedParamID = -1;
    public ushort SelectedParamID
    {
        get
        {
            return _selectedParamID >= 0 ? (ushort)_selectedParamID : throw new Exception("No parameters are selected");
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
    }

    public void CreateDebugParam()
    {
        CreateParameter(0, 1, 0);
    }

    public void CreatePointsForCurrentMesh()
    {
        ArtMesh selectedArtMesh = LayerManager.instance.SelectedArtMesh;
        if (_selectedParamID >= 0 && selectedArtMesh != null)
        {
            CreateParamPoints((ushort)_selectedParamID, selectedArtMesh.MeshID);
            HighlightCreatedCurves(selectedArtMesh.MeshID);
        }
    }

    public void CreateParameter(float min, float max, float defaultValue)
    {
        Parameter parameter = new(min, max, defaultValue);
        GameObject paramSlider = Instantiate(ParamSliderPrefab, ParamWidgetContent);
        paramSlider.GetComponent<ParameterSlider>().SetParamID(parameter.ID);

        paramSliders.Add(parameter.ID, paramSlider);
    }

    public void CreateParamPoints(ushort parameterID, ushort meshID)
    {
        Parameter parameter = ParameterRegistry.instance.GetParameter(parameterID);
        ParamCurve paramCurve = new(meshID, parameter.ID);
        ParamPoint minPoint = new(parameter.minValue);
        ParamPoint maxPoint = new(parameter.maxValue);
        ParamPoint midPoint = new((parameter.maxValue + parameter.minValue) / 2f);

        paramCurve.ParamPoints.Add(minPoint.ID);
        paramCurve.ParamPoints.Add(maxPoint.ID);
        paramCurve.ParamPoints.Add(midPoint.ID);

        parameter.ParamCurves.Add(paramCurve.ID);

        List<float> paramValues = new()
        {
            minPoint.ParamValue,
            maxPoint.ParamValue,
            midPoint.ParamValue
        };

        paramSliders[parameterID].GetComponent<ParameterSlider>().CreateParamPointHandles(paramValues);

        Debug.Log("created parampoints");
    }

    public void DeleteParamPointsOfMesh(ushort meshID)
    {
        ParameterRegistry.instance.DeleteAnimationDataOfMesh(meshID);
        HighlightCreatedCurves(meshID);
    }

    public void SelectParameter(ushort id)
    {
        if (_selectedParamID >= 0)
        {
            paramSliders[(ushort)_selectedParamID].GetComponent<ParameterSlider>().SetSelected(false);
        }

        paramSliders[id].GetComponent<ParameterSlider>().SetSelected(true);
        _selectedParamID = id;
    }

    public void HighlightCreatedCurves(ushort meshID)
    {
        List<ushort> assignedParams = ParameterRegistry.instance.GetAssignedParamIDsOfMesh(meshID);

        foreach (var item in paramSliders)
        {
            item.Value.GetComponent<ParameterSlider>().SetAssignedCurve(assignedParams.Contains(item.Key));
        }
    }

    public void UpdateAnimationData(object data, TransformType transformType, ushort meshID)
    {
        if (_selectedParamID < 0) return;

        ParameterRegistry parameterRegistry = ParameterRegistry.instance;

        Parameter currentParam = parameterRegistry.GetParameter((ushort)_selectedParamID);
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
                distFromPointValues[j] = Math.Abs(point.ParamValue - sliderValue);

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
                paramSliders[paramCurve.ParamID].GetComponent<ParameterSlider>().SetValue(paramPoints[minIndex].ParamValue);
                AnimationManager.instance.InterpolateParameter(paramPoints[minIndex].ParamValue, paramCurve.ParamID);
            }
        }

    }
}

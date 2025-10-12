using System.Collections.Generic;
using UnityEngine;

public class ParameterManager : MonoBehaviour
{
    public static ParameterManager instance;

    [SerializeField] private GameObject ParamSliderPrefab;
    [SerializeField] private Transform ParamWidgetContent;

    private readonly Dictionary<ushort, GameObject> paramSliders = new();

    private int _selectedParamID = -1;

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

    public void CreateParameter(int min, int max, int defaultValue)
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

        paramCurve.ParamPoints.Add(minPoint.ID);
        paramCurve.ParamPoints.Add(maxPoint.ID);

        parameter.ParamCurves.Add(paramCurve.ID);

        List<int> paramValues = new()
        {
            minPoint.ParamValue,
            maxPoint.ParamValue
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

    public void UpdateAnimationData(MeshData meshData, TransformType transformType)
    {
        if (_selectedParamID < 0) return;

        ParameterRegistry parameterRegistry = ParameterRegistry.instance;

        Parameter currentParam = parameterRegistry.GetParameter((ushort)_selectedParamID);
        List<ParamCurve> currentParamCurves = parameterRegistry.GetParamCurve(currentParam.ParamCurves);

        float sliderValue = paramSliders[currentParam.ID].GetComponent<ParameterSlider>().GetValue();
        for (int i = 0; i < currentParamCurves.Count; i++)
        {
            ParamCurve paramCurve = currentParamCurves[i];

            if (paramCurve.MeshID != meshData.ID) // filter by mesh id
                continue;

            List<ParamPoint> paramPoints = parameterRegistry.GetParamPoint(paramCurve.ParamPoints);

            foreach (ParamPoint point in paramPoints)
            {
                if (point.ParamValue != sliderValue) // filter by set parameter point values
                    continue;

                switch (transformType)
                {
                    case TransformType.POSITION:
                        point.Position = meshData.Position;
                        break;
                    case TransformType.ROTATION:
                        point.Rotation = meshData.Rotation;
                        break;
                    case TransformType.SCALE:
                        point.Scale = meshData.Scale;
                        break;
                }

                Debug.Log("updated point: " + point);
                break;
            }
        }
    }
}

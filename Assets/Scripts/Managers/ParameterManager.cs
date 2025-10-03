using System.Collections.Generic;
using UnityEngine;

public class ParameterManager : MonoBehaviour
{
    public static ParameterManager instance;

    [SerializeField] private GameObject ParamSliderPrefab;
    [SerializeField] private Transform ParamWidgetContent;

    private Dictionary<ushort, GameObject> paramSliders = new();

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
        ParamCurve paramCurve = new(meshID);
        ParamPoint minPoint = new(parameter.minValue);
        ParamPoint maxPoint = new(parameter.maxValue);

        paramCurve.ParamPoints.Add(minPoint.ID);
        paramCurve.ParamPoints.Add(maxPoint.ID);

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
}

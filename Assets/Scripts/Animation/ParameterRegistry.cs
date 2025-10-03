using System.Collections.Generic;
using UnityEngine;

public class ParameterRegistry : MonoBehaviour
{
    private readonly Dictionary<ushort, Parameter> Parameters = new();
    private readonly Dictionary<ushort, ParamCurve> ParamCurves = new();
    private readonly Dictionary<ushort, ParamPoint> ParamPoints = new();

    public static ParameterRegistry instance;

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

    public Parameter GetParameter(ushort id)
    {
        if (Parameters.ContainsKey(id))
        {
            return Parameters[id];
        }
        else
        {
            Debug.LogError("no such id in registry");
            throw new System.Exception();
        }
    }

    public void RegisterParameter(Parameter parameter)
    {
        if (Parameters.ContainsKey(parameter.ID))
        {
            Debug.LogError("duplicate id in registry");
        }

        Parameters.Add(parameter.ID, parameter);
    }

    public ParamCurve GetParamCurve(ushort id)
    {
        if (ParamCurves.ContainsKey(id))
        {
            return ParamCurves[id];
        }
        else
        {
            Debug.LogError("no such id in registry");
            throw new System.Exception();
        }
    }

    public void RegisterParamCurve(ParamCurve paramCurve)
    {
        if (ParamCurves.ContainsKey(paramCurve.ID))
        {
            Debug.LogError("duplicate id in registry");
        }

        ParamCurves.Add(paramCurve.ID, paramCurve);
    }

    public ParamPoint GetParamPoint(ushort id)
    {
        if (ParamPoints.ContainsKey(id))
        {
            return ParamPoints[id];
        }
        else
        {
            Debug.LogError("no such id in registry");
            throw new System.Exception();
        }
    }

    public void RegisterParamPoint(ParamPoint paramPoint)
    {
        if (ParamPoints.ContainsKey(paramPoint.ID))
        {
            Debug.LogError("duplicate id in registry");
        }

        ParamPoints.Add(paramPoint.ID, paramPoint);
    }
}

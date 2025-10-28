using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "ParameterRegistry", menuName = "Global/Parameter Registry")]
public class ParameterRegistry : ScriptableObject
{
    [SerializeField, HideInInspector]
    private Dictionary<Guid, Parameter> Parameters = new();
    [SerializeField, HideInInspector]
    private Dictionary<Guid, ParamCurve> ParamCurves = new();
    public Dictionary<Guid, ParamCurve> GetAllParamCurves => ParamCurves;
    [SerializeField, HideInInspector]
    private Dictionary<Guid, ParamPoint> ParamPoints = new();

    private static ParameterRegistry _instance;
    public static ParameterRegistry Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<ParameterRegistry>("ParameterRegistry");

                if (_instance == null)
                {
                    Debug.LogError("ParameterRegistry asset not found in Resources!");
                }
            }

            return _instance;
        }
    }

    public Parameter GetParameter(Guid id)
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

    public List<Parameter> GetAllParameters => Parameters.Select(p => p.Value).ToList();

    public void RegisterParameter(Parameter parameter)
    {
        if (Parameters.ContainsKey(parameter.ID))
        {
            Debug.LogError("duplicate id in registry");
        }

        Parameters.Add(parameter.ID, parameter);
    }

    public ParamCurve GetParamCurve(Guid id)
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

    public List<ParamCurve> GetParamCurve(List<Guid> ids)
    {
        List<ParamCurve> retCurves = new();

        for (int i = 0; i < ids.Count; i++)
        {
            Guid id = ids[i];
            if (ParamCurves.ContainsKey(id))
            {
                retCurves.Add(ParamCurves[id]);
            }
        }

        return retCurves;
    }

    public List<Guid> GetAssignedParamIDsOfMesh(Guid meshID)
    {
        List<Guid> retIDs = new();

        foreach ((_, ParamCurve paramCurve) in ParamCurves)
        {
            if (paramCurve.MeshID.Equals(meshID))
            {
                retIDs.Add(paramCurve.ParamID);
            }
        }

        return retIDs;
    }

    public void RegisterParamCurve(ParamCurve paramCurve)
    {
        if (ParamCurves.ContainsKey(paramCurve.ID))
        {
            Debug.LogError("duplicate id in registry");
        }

        ParamCurves.Add(paramCurve.ID, paramCurve);
    }

    public ParamPoint GetParamPoint(Guid id)
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

    public List<ParamPoint> GetParamPoint(List<Guid> ids)
    {
        List<ParamPoint> retCurves = new();

        for (int i = 0; i < ids.Count; i++)
        {
            Guid id = ids[i];
            if (ParamPoints.ContainsKey(id))
            {
                retCurves.Add(ParamPoints[id]);
            }
        }

        return retCurves;
    }

    public void RegisterParamPoint(ParamPoint paramPoint)
    {
        if (ParamPoints.ContainsKey(paramPoint.ID))
        {
            Debug.LogError("duplicate id in registry");
        }

        ParamPoints.Add(paramPoint.ID, paramPoint);
    }

    public void DeleteAnimationDataOfMesh(Guid id)
    {
        List<ParamCurve> paramCurves = GetParamCurve(GetAssignedParamIDsOfMesh(id));
        for (int i = 0; i < paramCurves.Count; i++)
        {
            ParamCurve paramCurve = paramCurves[i];
            foreach (Guid pointID in paramCurve.ParamPoints)
            {
                ParamPoints.Remove(pointID);
            }
            ParamCurves.Remove(paramCurve.ID);
        }
    }
}

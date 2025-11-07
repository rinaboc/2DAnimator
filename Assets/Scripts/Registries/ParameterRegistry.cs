using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "ParameterRegistry", menuName = "Global/Parameter Registry")]
public class ParameterRegistry : ScriptableObject
{
    [SerializeField] private Dictionary<Guid, Parameter> _parameters = new();
    [SerializeField] private Dictionary<Guid, ParamCurve> _paramCurves = new();
    [SerializeField] private Dictionary<Guid, ParamPoint> _paramPoints = new();

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

    public bool GetParameter(Guid id, out Parameter parameter) => _parameters.TryGetValue(id, out parameter);
    public bool RegisterParameter(Parameter parameter) => _parameters.TryAdd(parameter.ID, parameter);

    public List<Parameter> GetAllParameters => _parameters.Select(p => p.Value).ToList();


    public bool GetParamCurve(Guid id, out ParamCurve paramCurve) => _paramCurves.TryGetValue(id, out paramCurve);
    public bool RegisterParamCurve(ParamCurve paramCurve) => _paramCurves.TryAdd(paramCurve.ID, paramCurve);

    public List<ParamCurve> GetParamCurve(List<Guid> ids)
    {
        List<ParamCurve> retCurves = new();

        for (int i = 0; i < ids.Count; i++)
        {
            if (GetParamCurve(ids[i], out ParamCurve curve))
                retCurves.Add(curve);
        }

        return retCurves;
    }

    public List<Guid> GetAssignedParamIDsOfMesh(Guid meshID)
    {
        List<Guid> retIDs = new();

        foreach ((_, ParamCurve paramCurve) in _paramCurves)
        {
            if (paramCurve.MeshID.Equals(meshID))
            {
                retIDs.Add(paramCurve.ParamID);
            }
        }

        return retIDs;
    }


    public bool RegisterParamPoint(ParamPoint paramPoint) => _paramPoints.TryAdd(paramPoint.ID, paramPoint);
    public bool GetParamPoint(Guid id, out ParamPoint paramPoint) => _paramPoints.TryGetValue(id, out paramPoint);

    public List<ParamPoint> GetParamPoint(List<Guid> ids)
    {
        List<ParamPoint> retCurves = new();

        for (int i = 0; i < ids.Count; i++)
        {
            if (GetParamPoint(ids[i], out ParamPoint point))
                retCurves.Add(point);
        }

        return retCurves;
    }

    public void DeleteAnimationDataOfMesh(Guid id)
    {
        List<ParamCurve> paramCurves = GetParamCurve(GetAssignedParamIDsOfMesh(id));
        for (int i = 0; i < paramCurves.Count; i++)
        {
            ParamCurve paramCurve = paramCurves[i];
            foreach (Guid pointID in paramCurve.ParamPoints)
            {
                _paramPoints.Remove(pointID);
            }
            _paramCurves.Remove(paramCurve.ID);
        }
    }
}

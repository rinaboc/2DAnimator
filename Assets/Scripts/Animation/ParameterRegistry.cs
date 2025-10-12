using System.Collections.Generic;
using System.Linq;
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

    public Dictionary<ushort, ParamCurve> GetAllParamCurves()
    {
        return ParamCurves;
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

    public List<ParamCurve> GetParamCurve(List<ushort> ids)
    {
        List<ParamCurve> retCurves = new();

        for (int i = 0; i < ids.Count; i++)
        {
            ushort id = ids[i];
            if (ParamCurves.ContainsKey(id))
            {
                retCurves.Add(ParamCurves[id]);
            }
        }

        return retCurves;
    }

    public List<ushort> GetAssignedParamIDsOfMesh(ushort meshID)
    {
        List<ushort> retIDs = new();

        for (int i = 0; i < ParamCurves.Count; i++)
        {
            if (ParamCurves.ContainsKey((ushort)i) && ParamCurves[(ushort)i].MeshID == meshID)
            {
                retIDs.Add(ParamCurves[(ushort)i].ParamID);
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

    public List<ParamPoint> GetParamPoint(List<ushort> ids)
    {
        List<ParamPoint> retCurves = new();

        for (int i = 0; i < ids.Count; i++)
        {
            ushort id = ids[i];
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

    public void DeleteAnimationDataOfMesh(ushort id)
    {
        List<ParamCurve> paramCurves = GetParamCurve(GetAssignedParamIDsOfMesh(id));
        for (int i = 0; i < paramCurves.Count; i++)
        {
            ParamCurve paramCurve = paramCurves[i];
            foreach (ushort pointID in paramCurve.ParamPoints)
            {
                ParamPoints.Remove(pointID);
            }
            ParamCurves.Remove(paramCurve.ID);
        }
    }
}

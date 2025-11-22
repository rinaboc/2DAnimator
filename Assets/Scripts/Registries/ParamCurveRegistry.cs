using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ParamCurveRegistry", menuName = "Global/ParamCurveRegistry")]
public class ParamCurveRegistry : RegistryBase<ParamCurve>
{
    private static ParamCurveRegistry _instance;
    public static ParamCurveRegistry Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<ParamCurveRegistry>("ParamCurveRegistry");

                if (_instance == null)
                {
                    Debug.LogError("ParamCurveRegistry asset not found in Resources!");
                }
            }

            return _instance;
        }
    }

    public List<Guid> GetAssignedParamIDsOfMesh(Guid meshID)
    {
        List<Guid> retIDs = new();

        foreach ((_, ParamCurve paramCurve) in _map)
        {
            if (paramCurve.MeshID.Equals(meshID))
            {
                retIDs.Add(paramCurve.ParamID);
            }
        }

        return retIDs;
    }

    /// <summary>
    /// Get the ParamCurve entries that are assigned to a given mesh.
    /// </summary>
    public List<ParamCurve> GetParamCurvesOfMesh(Guid meshID)
    {
        List<ParamCurve> ret = new();

        foreach ((_, ParamCurve paramCurve) in _map)
        {
            if (paramCurve.MeshID.Equals(meshID))
            {
                ret.Add(paramCurve);
            }
        }

        return ret;
    }
}

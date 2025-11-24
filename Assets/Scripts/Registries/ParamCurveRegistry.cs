using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ParamCurveRegistry", menuName = "Global/ParamCurveRegistry")]
public class ParamCurveRegistry : RegistryBase<ParamCurve, ParamCurveRegistry>
{
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

    public override void SaveState(ref SaveData saveData)
    {
        throw new NotImplementedException();
    }
}

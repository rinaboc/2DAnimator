using System;
using System.Collections.Generic;

[Serializable]
public class ParamCurve : EntityBase
{
    public Guid MeshID;
    public Guid ParamID;

    public readonly List<Guid> ParamPoints;

    public ParamCurve(Guid meshID, Guid paramID) : base()
    {
        MeshID = meshID;
        ParamID = paramID;
        ParamPoints = new();
    }
}

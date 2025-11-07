
using System;
using System.Collections.Generic;

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

    protected override void Register()
    {
        ParamCurveRegistry.Instance.Register(this);
    }
}

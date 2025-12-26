
using System;
using System.Collections.Generic;

[Serializable]
public class ParamCurve : EntityBase
{
    public Guid MeshID;
    public Guid ParamID;

    public readonly List<Guid> ParamPoints;

    public ParamCurve(Guid meshID, Guid paramID, bool autoRegister = true) : base(autoRegister)
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

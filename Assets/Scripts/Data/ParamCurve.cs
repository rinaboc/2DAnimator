
using System;
using System.Collections.Generic;

public class ParamCurve
{
    public Guid ID;
    public Guid MeshID;
    public Guid ParamID;

    public readonly List<Guid> ParamPoints;

    public ParamCurve(Guid meshID, Guid paramID, bool autoRegister = true)
    {
        ID = Guid.NewGuid();
        MeshID = meshID;
        ParamID = paramID;
        ParamPoints = new();

        if (autoRegister) Register();
    }

    public void Register()
    {
        ParameterRegistry.Instance.RegisterParamCurve(this);
    }
}

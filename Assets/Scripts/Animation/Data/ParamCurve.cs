
using System.Collections.Generic;

public class ParamCurve
{
    private static ushort _paramCurveCounter = 0;

    public ushort ID;
    public ushort MeshID;
    public ushort ParamID;

    public readonly List<ushort> ParamPoints;

    public ParamCurve(ushort meshID, ushort paramID, bool autoRegister = true)
    {
        ID = _paramCurveCounter++;
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

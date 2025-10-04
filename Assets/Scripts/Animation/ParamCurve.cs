
using System.Collections.Generic;

public class ParamCurve
{
    private static ushort _paramCurveCounter = 0;

    public ushort ID;
    public ushort MeshID;

    public readonly List<ushort> ParamPoints;

    public ParamCurve(ushort meshID, bool autoRegister = true)
    {
        ID = _paramCurveCounter++;
        MeshID = meshID;
        ParamPoints = new();

        if (autoRegister) Register();
    }

    public void Register()
    {
        ParameterRegistry.instance.RegisterParamCurve(this);
    }
}

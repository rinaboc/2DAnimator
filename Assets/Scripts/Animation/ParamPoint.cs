using UnityEngine;

public class ParamPoint
{
    private static ushort _paramPtCounter = 0;

    public readonly ushort ID;
    public int ParamValue;

    public Vector3 Position { get; set; }
    public Vector3 Rotation { get; set; }
    public Vector3 Scale { get; set; }

    public ParamPoint(int paramValue, bool autoRegister = true)
    {
        ID = _paramPtCounter++;
        this.ParamValue = paramValue;

        if (autoRegister) Register();
    }

    public void Register()
    {
        ParameterRegistry.instance.RegisterParamPoint(this);
    }

}

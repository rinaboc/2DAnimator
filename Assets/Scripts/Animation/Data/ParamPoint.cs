using System;
using UnityEngine;

public class ParamPoint
{
    private static ushort _paramPtCounter = 0;

    public readonly ushort ID;
    public float ParamValue;

    public Vector3 Position { get; set; }
    public Quaternion Rotation { get; set; }
    public Vector3 Scale { get; set; }

    public ParamPoint(float paramValue, bool autoRegister = true)
    {
        ID = _paramPtCounter++;
        this.ParamValue = paramValue;
        Rotation = Quaternion.identity;

        if (autoRegister) Register();
    }

    public void Register()
    {
        ParameterRegistry.Instance.RegisterParamPoint(this);
    }

    public override string ToString() => $"{ID}: paramValue {ParamValue}, position {Position}, rotation {Rotation}, scale {Scale}";

    public float Dist(ParamPoint pp) => Math.Abs(this.ParamValue - pp.ParamValue);
    public float Dist(float value) => Math.Abs(this.ParamValue - value);
}

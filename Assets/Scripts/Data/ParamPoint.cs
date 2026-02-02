using System;

[Serializable]
public class ParamPoint : EntityBase
{
    public float ParamValue;
    public TransformData transform;

    public ParamPoint(float paramValue) : base()
    {
        ParamValue = paramValue;
        transform = new();
    }

    public override string ToString() => $"{ID}: paramValue {ParamValue}, {transform}";

    public float Dist(ParamPoint pp) => Math.Abs(this.ParamValue - pp.ParamValue);
    public float Dist(float value) => Math.Abs(this.ParamValue - value);
}

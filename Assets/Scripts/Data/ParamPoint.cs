using System;

[Serializable]
public class ParamPoint : EntityBase
{
    public float ParamValue;
    public TransformData transform;

    public ParamPoint(float paramValue) : base()
    {
        this.ParamValue = paramValue;
        transform = new();
    }

    protected override void Register()
    {
        ParamPointRegistry.Instance.Register(this);
    }

    public override string ToString() => $"{ID}: paramValue {ParamValue}, {transform}";

    public float Dist(ParamPoint pp) => Math.Abs(this.ParamValue - pp.ParamValue);
    public float Dist(float value) => Math.Abs(this.ParamValue - value);
}

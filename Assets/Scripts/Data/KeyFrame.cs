using System;

[Serializable]
public class KeyFrame : EntityBase
{
    public Guid ParamID;
    public float ParamValue;
    public int Frame;

    public KeyFrame(Guid ParamID, float ParamValue, int Frame, bool autoRegister = true) : base(autoRegister)
    {
        this.ParamID = ParamID;
        this.ParamValue = ParamValue;
        this.Frame = Frame;
    }

    protected override void Register()
    {
        KeyFrameRegistry.Instance.Register(this);
    }

    public bool IsSameCell(KeyFrame kf) => this.ParamID.Equals(kf.ParamID) && this.Frame == kf.Frame;

    public override string ToString() => $"paramID: {ParamID}, value: {ParamValue} at frame: {Frame}";
}

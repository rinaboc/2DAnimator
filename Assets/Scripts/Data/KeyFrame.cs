using System;

public class KeyFrame
{
    public Guid ID;
    public Guid ParamID;
    public float ParamValue;
    public int Frame;

    public KeyFrame(Guid ParamID, float ParamValue, int Frame, bool autoRegister = true)
    {
        ID = Guid.NewGuid();
        this.ParamID = ParamID;
        this.ParamValue = ParamValue;
        this.Frame = Frame;

        if (autoRegister)
        {
            Register();
        }
    }

    private void Register()
    {
        KeyFrameRegistry.Instance.AddKeyframe(this);
    }

    public bool IsSameCell(KeyFrame kf) => this.ParamID.Equals(kf.ParamID) && this.Frame == kf.Frame;

    public override string ToString() => $"paramID: {ParamID}, value: {ParamValue} at frame: {Frame}";
}

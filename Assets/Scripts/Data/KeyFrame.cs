using System;

[Serializable]
public class KeyFrame : EntityBase
{
    public Guid ParamID;
    public float ParamValue;
    public int Frame;

    public KeyFrame(Guid ParamID, float ParamValue, int Frame)
    {
        this.ParamID = ParamID;
        this.ParamValue = ParamValue;
        this.Frame = Frame;
    }

    public bool IsSameCell(KeyFrame kf) => this.ParamID.Equals(kf.ParamID) && this.Frame == kf.Frame;

    public override string ToString() => $"paramID: {ParamID}, value: {ParamValue} at frame: {Frame}";
}

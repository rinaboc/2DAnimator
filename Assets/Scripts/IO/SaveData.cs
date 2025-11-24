using UnityEngine;

[System.Serializable]
public class SaveData
{
    public MeshData[] MeshDatas { get; set; }
    public Parameter[] Parameters { get; set; }
    public ParamCurve[] ParamCurves { get; set; }
    public ParamPoint[] ParamPoints { get; set; }
    public KeyFrame[] KeyFrames { get; set; }
}

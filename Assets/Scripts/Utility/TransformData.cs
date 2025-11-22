using UnityEngine;

[System.Serializable]
public class TransformData
{
    public Vector3 Position { get; set; }
    public Quaternion Rotation { get; set; }
    public Vector3 Scale { get; set; }

    public TransformData()
    {
        Position = Vector3.zero;
        Rotation = Quaternion.identity;
        Scale = Vector3.one;
    }

    public override string ToString() => $"position {Position}, rotation {Rotation}, scale {Scale}";
}

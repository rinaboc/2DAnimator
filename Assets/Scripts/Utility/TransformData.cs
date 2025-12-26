using UnityEngine;

[System.Serializable]
public class TransformData
{
    [SerializeField] private float[] position = new float[3];
    [SerializeField] private float[] rotation = new float[4];
    [SerializeField] private float[] scale = new float[3];

    public Vector3 Position
    {
        get => new(position[0], position[1], position[2]);
        set { position[0] = value.x; position[1] = value.y; position[2] = value.z; }
    }

    public Quaternion Rotation
    {
        get => new(rotation[0], rotation[1], rotation[2], rotation[3]);
        set { rotation[0] = value.x; rotation[1] = value.y; rotation[2] = value.z; rotation[3] = value.w; }
    }

    public Vector3 Scale
    {
        get => new(scale[0], scale[1], scale[2]);
        set { scale[0] = value.x; scale[1] = value.y; scale[2] = value.z; }
    }

    public TransformData()
    {
        Position = Vector3.zero;
        Rotation = Quaternion.identity;
        Scale = Vector3.zero;
    }

    public static TransformData operator +(TransformData a, TransformData b)
    {
        TransformData result = new()
        {
            Position = a.Position + b.Position,
            Rotation = a.Rotation * b.Rotation,
            Scale = a.Scale + b.Scale
        };
        return result;
    }

    public override string ToString() => $"position {Position}, rotation {Rotation}, scale {Scale}";
}

using System;
using System.Runtime.Serialization;
using Assets.Scripts.Utility;
using UnityEngine;

[Serializable]
public class MeshData : EntityBase
{
    private static ushort _objCounter = 0;
    public ushort drawOrder;
    public string sourcePath;
    public string name;

    public TransformData transform;
    public SerializableTexture texture;

    public MeshData(string sourcePath) : base()
    {
        drawOrder = ++_objCounter;
        this.sourcePath = sourcePath;
        name = "ArtObject" + ID;
        transform = new()
        {
            Scale = Vector3.one
        };
    }

    [OnDeserialized]
    private void OnDeserialized(StreamingContext context)
    {
        _objCounter++;
    }

    public override string ToString() => $"{ID}: {name}, {drawOrder}, {sourcePath}";

}

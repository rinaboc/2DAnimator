using System;
using UnityEngine;

public class MeshData : EntityBase
{
    private static ushort _objCounter = 0;
    public ushort drawOrder;
    public string sourcePath;
    public string name;

    public Vector3 Position { get; set; }
    public Quaternion Rotation { get; set; }
    public Vector3 Scale { get; set; }

    public MeshData(string sourcePath) : base()
    {
        drawOrder = ++_objCounter;
        this.sourcePath = sourcePath;
        name = "ArtObject" + ID;

        Rotation = Quaternion.identity;
        Scale = Vector3.one;
    }

    protected override void Register()
    {
        MeshRegistry.Instance.RegisterMeshData(this);
    }

    public override string ToString() => $"{ID}: {name}, {drawOrder}, {sourcePath}";

}

using System;
using System.Runtime.Serialization;
using Assets.Scripts.Data.MeshInfo;
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
    [NonSerialized] public MeshInfo meshInfo;
    private MeshInfoData _meshInfoSerialized;

    public MeshData(string sourcePath) : base()
    {
        drawOrder = ++_objCounter;
        this.sourcePath = sourcePath;
        name = "ArtObject" + ID;
        transform = new()
        {
            Scale = Vector3.one
        };
        meshInfo = new();
    }

    [OnDeserialized]
    private void OnDeserialized(StreamingContext context)
    {
        _objCounter++;

        if (_meshInfoSerialized != null)
        {
            meshInfo = _meshInfoSerialized.ToMeshInfo();
        }
    }

    [OnSerializing]
    private void OnSerializing(StreamingContext context)
    {
        if (meshInfo != null)
        {
            _meshInfoSerialized = meshInfo.ToData();
        }
    }

    public override string ToString() => $"{ID}: {name}, {drawOrder}, {sourcePath}";

}

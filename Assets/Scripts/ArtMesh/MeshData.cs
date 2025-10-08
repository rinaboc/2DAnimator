using UnityEngine;

public class MeshData
{
    private static ushort _objCounter = 0;
    public readonly ushort ID;
    public ushort drawOrder;
    public string sourcePath;
    public string name;

    public Vector3 Position { get; set; }
    public Quaternion Rotation { get; set; }
    public Vector3 Scale { get; set; }

    public MeshData(string sourcePath)
    {
        ID = ++_objCounter;
        drawOrder = ID;
        this.sourcePath = sourcePath;
        name = "ArtObject" + ID;

        Scale = Vector3.one;
    }

    public override string ToString() => $"{ID}: {name}, {drawOrder}, {sourcePath}";

}

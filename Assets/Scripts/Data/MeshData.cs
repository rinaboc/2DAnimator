using System;

[Serializable]
public class MeshData : EntityBase
{
    private static ushort _objCounter = 0;
    public ushort drawOrder;
    public string sourcePath;
    public string name;

    public TransformData transform;

    public MeshData(string sourcePath) : base()
    {
        drawOrder = ++_objCounter;
        this.sourcePath = sourcePath;
        name = "ArtObject" + ID;
        transform = new();
    }

    protected override void Register()
    {
        MeshRegistry.Instance.Register(this);
    }

    public override string ToString() => $"{ID}: {name}, {drawOrder}, {sourcePath}";

}

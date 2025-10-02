public struct MeshData
{
    private static ushort _objCounter = 0;
    public readonly ushort ID;
    public ushort drawOrder;
    public string sourcePath;
    public string name;

    public MeshData(string sourcePath)
    {
        ID = ++_objCounter;
        drawOrder = ID;
        this.sourcePath = sourcePath;
        name = "ArtObject" + ID;
    }

    public override readonly string ToString() => $"{ID}: {name}, {drawOrder}, {sourcePath}";

}

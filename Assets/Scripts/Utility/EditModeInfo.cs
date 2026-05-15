using Assets.Scripts.Data.MeshInfo;

public enum EditTool
{
    SELECT, CREATE, DELETE
}

public class EditModeInfo
{
    public bool IsEditMode { get; set; }
    public MeshInfo CurrentTopology { get; set; }
    public Vertex SelectedVertex { get; set; }
    public EditTool CurrentTool { get; set; }

    public EditModeInfo()
    {
        IsEditMode = false;
        CurrentTopology = null;
        SelectedVertex = null;
        CurrentTool = EditTool.SELECT;
    }
}

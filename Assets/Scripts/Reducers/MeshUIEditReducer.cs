using Assets.Scripts.States.EditMode;
using Assets.Scripts.Utility.MVI;

public class MeshUIEditReducer : IReducer<MeshUIEditState>
{
    public MeshUIEditState Reduce(MeshUIEditState previous, IIntent intent)
    {
        return intent switch
        {
            _ => previous
        };
    }

    public MeshUIEditState Update(MeshUIEditState previous, IModelContext context)
    {
        if (!context.Meshes.TryGet(context.SessionInfo.SelectedMeshID, out var mesh)) return previous;

        var next = previous.Copy();
        next.Topology = mesh.meshInfo;
        return next;
    }
}

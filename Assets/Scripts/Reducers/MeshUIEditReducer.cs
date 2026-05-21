using System;
using Assets.Scripts.States.EditMode;
using Assets.Scripts.Utility.MVI;

public class MeshUIEditReducer : IReducer<MeshUIEditState>
{
    public MeshUIEditState Reduce(MeshUIEditState previous, IIntent intent)
    {
        return intent switch
        {
            ToggleDebugIntent toggle => ReduceToggleDebug(previous, toggle),
            _ => previous
        };
    }

    private MeshUIEditState ReduceToggleDebug(MeshUIEditState previous, ToggleDebugIntent toggle)
    {
        var next = previous.Copy();
        next.isDebugDraw = toggle.Toggle;
        return next;
    }

    public MeshUIEditState Update(MeshUIEditState previous, IModelContext context)
    {
        if (!context.Meshes.TryGet(context.SessionInfo.SelectedMeshID, out var mesh)) return previous;

        var next = previous.Copy();
        next.OriginalTopology = mesh.meshInfo;
        next.CurrentTopology = context.SessionInfo.EditModeInfo.CurrentTopology;
        next.SelectedVertex = context.SessionInfo.EditModeInfo.SelectedVertex;
        return next;
    }
}

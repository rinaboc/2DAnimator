using System;
using Assets.Scripts.Data.MeshInfo;
using Assets.Scripts.Utility.MVI;
using UnityEngine;

public class MeshUIEditCommandHandler : ICommandHandler
{
    public Action Execute(IIntent intent, IModelContext context)
    {
        return intent switch
        {
            StartEditModeIntent _ => ExecuteStartEditMode(context),
            EndEditModeIntent exit => ExecuteEndEditMode(exit, context),
            _ => null
        };
    }

    private Action ExecuteEndEditMode(EndEditModeIntent exit, IModelContext context)
    {
        if (!context.Meshes.TryGet(context.SessionInfo.SelectedMeshID, out MeshData mesh)) return null;

        MeshManager.Instance.DeleteMeshEditObj();

        if (!exit.SaveRequired) return null;

        // TODO: save changes

        return null;
    }

    private Action ExecuteStartEditMode(IModelContext context)
    {
        if (!context.Meshes.TryGet(context.SessionInfo.SelectedMeshID, out MeshData mesh)) return null;

        GameObject newMeshObject = MeshBuilder.Build(mesh.texture.Data, mesh.meshInfo);
        MeshManager.Instance.CreateMeshEditObj(newMeshObject, mesh.ID);

        return null;
    }


}

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
            EditSelectVertexIntent select => ExecuteEditSelectVertex(select, context),
            EditMoveVertexIntent move => ExecuteEditMoveVertex(move, context),
            EditMoveVertexEndedIntent _ => ExecuteEditMoveVertexEnded(context),
            _ => null
        };
    }

    private Action ExecuteEditMoveVertexEnded(IModelContext context)
    {
        return null;
    }

    private Action ExecuteEditMoveVertex(EditMoveVertexIntent move, IModelContext context)
    {
        if (!context.SessionInfo.IsEditMode || context.SessionInfo.SelectedVertex == null) return null;

        context.SessionInfo.SelectedVertex.Position = move.ClickWorldPos;

        return null;
    }

    public Vertex GetVertexAtPos(Vector2 pos, float radius, MeshInfo meshInfo)
    {
        float minSqrDist = radius * radius;
        Vertex closest = null;

        foreach (var v in meshInfo.Vertices)
        {
            float pixelDist = (v.Position - pos).sqrMagnitude;
            if (pixelDist < minSqrDist)
            {
                minSqrDist = pixelDist;
                closest = v;
            }
        }
        return closest;
    }

    private Action ExecuteEditSelectVertex(EditSelectVertexIntent select, IModelContext context)
    {
        if (!context.SessionInfo.IsEditMode) return null;

        var previousVertex = context.SessionInfo.SelectedVertex;
        context.SessionInfo.SelectedVertex = GetVertexAtPos(select.ClickWorldPos, 0.25f, context.SessionInfo.CurrentTopology);

        return () => context.SessionInfo.SelectedVertex = previousVertex;
    }

    private Action ExecuteEndEditMode(EndEditModeIntent exit, IModelContext context)
    {
        if (!context.Meshes.TryGet(context.SessionInfo.SelectedMeshID, out MeshData mesh)) return null;

        MeshManager.Instance.DeleteMeshEditObj();

        if (exit.SaveRequired)
        {
            var texMin = context.SessionInfo.TexMin;
            var texSize = context.SessionInfo.TexSize;

            foreach (var v in context.SessionInfo.CurrentTopology.Vertices)
            {
                float tx = (v.Position.x - texMin.x) / texSize.x;
                float ty = (v.Position.y - texMin.y) / texSize.y;

                v.UV = new Vector2(tx, ty);
            }
            mesh.meshInfo = context.SessionInfo.CurrentTopology.Clone();

            MeshManager.Instance.DeleteArtMeshObj(mesh.ID);
            GameObject rebuiltMeshObject = MeshBuilder.Build(mesh.texture.Data, mesh.meshInfo, out _, out _);
            var boxCollider = rebuiltMeshObject.GetComponent<BoxCollider>();
            rebuiltMeshObject.transform.position -= boxCollider.bounds.center;
            boxCollider.center = Vector3.zero;

            MeshManager.Instance.CreateArtMeshObj(rebuiltMeshObject, mesh.ID);
        }

        context.SessionInfo.CurrentTopology = null;
        context.SessionInfo.SelectedVertex = null;

        return null;
    }

    private Action ExecuteStartEditMode(IModelContext context)
    {
        if (!context.Meshes.TryGet(context.SessionInfo.SelectedMeshID, out MeshData mesh)) return null;

        GameObject newMeshObject = MeshBuilder.Build(mesh.texture.Data, mesh.meshInfo, out var min, out var size);
        MeshManager.Instance.CreateMeshEditObj(newMeshObject, mesh.ID);
        context.SessionInfo.CurrentTopology = mesh.meshInfo.Clone();
        context.SessionInfo.TexMin = min;
        context.SessionInfo.TexSize = size;
        context.SessionInfo.SelectedVertex = null;

        return null;
    }


}

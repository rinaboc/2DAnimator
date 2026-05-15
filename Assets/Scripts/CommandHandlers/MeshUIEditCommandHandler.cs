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
            EditClickIntent select => ExecuteEditClick(select, context),
            EditDragIntent move => ExecuteEditDrag(move, context),
            EditDragEndIntent _ => ExecuteEditDragEnd(context),
            _ => null
        };
    }

    private Action ExecuteEditDragEnd(IModelContext context)
    {
        return null;
    }

    private Action ExecuteEditDrag(EditDragIntent move, IModelContext context)
    {
        if (!context.SessionInfo.EditModeInfo.IsEditMode || context.SessionInfo.EditModeInfo.SelectedVertex == null) return null;

        context.SessionInfo.EditModeInfo.SelectedVertex.Position = move.ClickWorldPos;

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

    private Action ExecuteEditClick(EditClickIntent select, IModelContext context)
    {
        if (!context.SessionInfo.EditModeInfo.IsEditMode) return null;

        var currentTool = context.SessionInfo.EditModeInfo.CurrentTool;

        if (currentTool == EditTool.CREATE)
        {
            var oldTopology = context.SessionInfo.EditModeInfo.CurrentTopology;
            var newTopology = TopologyBuilder.InsertVertex(context.SessionInfo.EditModeInfo.CurrentTopology, select.ClickWorldPos);

            context.SessionInfo.EditModeInfo.CurrentTopology = newTopology;

            return () => context.SessionInfo.EditModeInfo.CurrentTopology = oldTopology;
        }

        Vertex selectedVertex = GetVertexAtPos(select.ClickWorldPos, 0.25f, context.SessionInfo.EditModeInfo.CurrentTopology);

        if (currentTool == EditTool.SELECT)
        {
            var previousVertex = context.SessionInfo.EditModeInfo.SelectedVertex;
            context.SessionInfo.EditModeInfo.SelectedVertex = selectedVertex;

            return () => context.SessionInfo.EditModeInfo.SelectedVertex = previousVertex;
        }


        if (currentTool == EditTool.DELETE)
        {
            var oldTopology = context.SessionInfo.EditModeInfo.CurrentTopology;

            if (selectedVertex == null) return null;
            var newTopology = TopologyBuilder.RemoveVertex(context.SessionInfo.EditModeInfo.CurrentTopology, selectedVertex);

            context.SessionInfo.EditModeInfo.CurrentTopology = newTopology;

            return () => context.SessionInfo.EditModeInfo.CurrentTopology = oldTopology;
        }

        return null;
    }

    private Action ExecuteEndEditMode(EndEditModeIntent exit, IModelContext context)
    {
        if (!context.Meshes.TryGet(context.SessionInfo.SelectedMeshID, out MeshData mesh)) return null;

        MeshManager.Instance.DeleteMeshEditObj();

        if (exit.SaveRequired)
        {
            var texMin = context.SessionInfo.TexMin;
            var texSize = context.SessionInfo.TexSize;

            var updatedMeshInfo = context.SessionInfo.EditModeInfo.CurrentTopology.Clone();
            foreach (var v in updatedMeshInfo.Vertices)
            {
                float tx = (v.Position.x - texMin.x) / texSize.x;
                float ty = (v.Position.y - texMin.y) / texSize.y;

                v.UV = new Vector2(tx, ty);
            }
            mesh.meshInfo = updatedMeshInfo;

            MeshManager.Instance.DeleteArtMeshObj(mesh.ID);
            GameObject rebuiltMeshObject = MeshBuilder.Build(mesh.texture.Data, mesh.meshInfo);
            MeshManager.Instance.CreateArtMeshObj(rebuiltMeshObject, mesh.ID);
        }

        context.SessionInfo.EditModeInfo.CurrentTopology = null;
        context.SessionInfo.EditModeInfo.SelectedVertex = null;

        return null;
    }

    private Action ExecuteStartEditMode(IModelContext context)
    {
        if (!context.Meshes.TryGet(context.SessionInfo.SelectedMeshID, out MeshData mesh)) return null;

        GameObject newMeshObject = MeshBuilder.Build(mesh.texture.Data, mesh.meshInfo, out var min, out var size);

        var boxCollider = newMeshObject.GetComponent<BoxCollider>();
        var topLeft = ViewportManager.Instance.TopLeftAnchor;
        var bottomRight = ViewportManager.Instance.BottomRightAnchor;
        boxCollider.size = new Vector3(Math.Abs(bottomRight.x - topLeft.x), Math.Abs(bottomRight.y - topLeft.y), 0);

        MeshManager.Instance.CreateMeshEditObj(newMeshObject, mesh.ID);
        context.SessionInfo.EditModeInfo.CurrentTopology = mesh.meshInfo.Clone();
        context.SessionInfo.TexMin = min;
        context.SessionInfo.TexSize = size;
        context.SessionInfo.EditModeInfo.SelectedVertex = null;

        return null;
    }


}

using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.States;
using Assets.Scripts.Utility.MVI;
using UnityEngine;

public class MeshLayerReducer : IReducer<MeshLayerStates>
{
    public MeshLayerStates Reduce(MeshLayerStates previous, IIntent intent)
    {
        return intent switch
        {
            // ChangeLayerNameIntent change => ReduceChangeLayerName(previous, change),
            SelectLayerIntent select => ReduceSelectLayer(previous, select),
            UpdateTransformIntent update => ReduceUpdateTransform(previous, update),
            // SaveTransformIntent save => ReduceSaveTransform(previous, save),
            // InterpolateTransformIntent interpolate => ReduceInterpolateTransform(previous, interpolate),
            // ResetInterpolationIntent reset => ReduceResetInterpolation(previous, reset),
            // CreateMeshLayerIntent create => ReduceCreateMeshLayer(previous, create),
            // MoveLayerDownIntent _ => ReduceMoveLayerDown(previous),
            // MoveLayerUpIntent _ => ReduceMoveLayerUp(previous),
            // DeleteLayerIntent _ => ReduceDeleteLayer(previous),
            // InitializeProjectIntent init => ReduceInitializeProject(previous, init),
            // CreateParamPointsIntent _ => ReduceCreateParamPoints(previous),
            _ => previous
        };
    }

    private MeshLayerStates ReduceCreateParamPoints(MeshLayerStates previous)
    {
        if (previous.SelectedMeshLayerID == Guid.Empty) return previous;

        var next = previous.Clone();
        next.MeshLayers[previous.SelectedMeshLayerID].HasParametersAssigned = true;
        return next;
    }

    private MeshLayerStates ReduceChangeLayerName(MeshLayerStates previous, ChangeLayerNameIntent change)
    {
        return new()
        {
            MeshLayers = previous.MeshLayers.ToDictionary(
                p => p.Key,
                p => new MeshLayerState(p.Value)
                {
                    Name = p.Key.Equals(change.LayerID) ? change.NewName : p.Value.Name
                }
            ),
            SelectedMeshLayerID = previous.SelectedMeshLayerID
        };
    }

    private MeshLayerStates ReduceSelectLayer(MeshLayerStates previous, SelectLayerIntent select)
    {
        return new()
        {
            MeshLayers = previous.MeshLayers.ToDictionary(
                p => p.Key,
                p => new MeshLayerState(p.Value)
                {
                    IsSelected = p.Key == select.LayerID
                }
            ),
            SelectedMeshLayerID = select.LayerID
        };
    }

    private MeshLayerStates ReduceUpdateTransform(MeshLayerStates previous, UpdateTransformIntent update)
    {
        bool areParametersAssigned = previous.MeshLayers[update.MeshID].HasParametersAssigned;
        if (!areParametersAssigned) return previous;

        var next = previous.Clone();
        var mesh = next.MeshLayers[update.MeshID];

        switch (update.Type)
        {
            case TransformType.POSITION:
                mesh.AnimationTransform.Position = update.Data.Position - mesh.MeshTransform.Position;
                break;
            case TransformType.ROTATION:
                mesh.AnimationTransform.Rotation = Quaternion.Inverse(mesh.MeshTransform.Rotation) * update.Data.Rotation;
                break;
            case TransformType.SCALE:
                mesh.AnimationTransform.Scale = update.Data.Scale - mesh.MeshTransform.Scale;
                break;
        }
        return next;
    }

    private MeshLayerStates ReduceSaveTransform(MeshLayerStates previous, SaveTransformIntent save)
    {
        return previous;
    }

    private MeshLayerStates ReduceInterpolateTransform(MeshLayerStates previous, InterpolateTransformIntent interpolate)
    {
        var next = previous.Clone();

        foreach ((Guid meshID, TransformData delta) in interpolate.Deltas)
        {
            if (!next.MeshLayers.TryGetValue(meshID, out MeshLayerState mesh)) continue;
            mesh.AnimationTransform = delta;
            mesh.InterpolatedTransform = mesh.MeshTransform + delta;
            mesh.IsInterpolated = true;
        }

        return next;
    }

    private MeshLayerStates ReduceResetInterpolation(MeshLayerStates previous, ResetInterpolationIntent reset)
    {
        return new()
        {
            MeshLayers = previous.MeshLayers.ToDictionary(
                p => p.Key,
                p => new MeshLayerState(p.Value)
                {
                    IsInterpolated = false
                }
            ),
            SelectedMeshLayerID = previous.SelectedMeshLayerID
        };
    }

    private MeshLayerStates ReduceCreateMeshLayer(MeshLayerStates previous, CreateMeshLayerIntent create)
    {
        var next = previous.Clone();

        next.MeshLayers[create.ID] = new()
        {
            ID = create.ID,
            Name = "Layer " + next.MeshLayers.Count,
            Texture = create.Tex,
            SourcePath = create.Path,
            MeshTransform = new TransformData() { Scale = Vector3.one },
            AnimationTransform = new TransformData(),
            DrawOrder = (ushort)next.MeshLayers.Count
        };

        return next;
    }

    private MeshLayerStates ReduceMoveLayerUp(MeshLayerStates previous)
    {
        var next = previous.Clone();
        MeshLayerState selectedMesh = next.MeshLayers[previous.SelectedMeshLayerID];
        ushort inf = 0;
        Guid swapID = Guid.Empty;
        foreach ((_, MeshLayerState meshLayerState) in next.MeshLayers)
        {
            if (meshLayerState.DrawOrder < selectedMesh.DrawOrder && meshLayerState.DrawOrder >= inf)
            {
                inf = (ushort)meshLayerState.DrawOrder;
                swapID = meshLayerState.ID;
            }
        }

        if (swapID != Guid.Empty)
        {
            MeshLayerState swappedMesh = next.MeshLayers[swapID];
            (swappedMesh.DrawOrder, selectedMesh.DrawOrder) = (selectedMesh.DrawOrder, swappedMesh.DrawOrder);
        }
        else
            return previous;

        return next;
    }

    private MeshLayerStates ReduceMoveLayerDown(MeshLayerStates previous)
    {
        var next = previous.Clone();
        MeshLayerState selectedMesh = next.MeshLayers[previous.SelectedMeshLayerID];
        ushort inf = ushort.MaxValue;
        Guid swapID = Guid.Empty;
        foreach ((_, MeshLayerState meshLayerState) in next.MeshLayers)
        {
            if (meshLayerState.DrawOrder > selectedMesh.DrawOrder && meshLayerState.DrawOrder < inf)
            {
                inf = (ushort)meshLayerState.DrawOrder;
                swapID = meshLayerState.ID;
            }
        }

        if (swapID != Guid.Empty)
        {
            MeshLayerState swappedMesh = next.MeshLayers[swapID];
            (swappedMesh.DrawOrder, selectedMesh.DrawOrder) = (selectedMesh.DrawOrder, swappedMesh.DrawOrder);
        }
        else
            return previous;

        return next;
    }

    private MeshLayerStates ReduceDeleteLayer(MeshLayerStates previous)
    {
        if (previous.SelectedMeshLayerID == Guid.Empty) return previous;

        var next = previous.Clone();
        next.MeshLayers.Remove(previous.SelectedMeshLayerID);
        next.SelectedMeshLayerID = Guid.Empty;

        var orderedLayers = next.MeshLayers.Values
            .OrderBy(layer => layer.DrawOrder)
            .ToList();
        for (int i = 0; i < orderedLayers.Count; i++)
        {
            orderedLayers[i].DrawOrder = (ushort)i;
        }
        next.MeshLayers = orderedLayers.ToDictionary(layer => layer.ID);

        return next;
    }

    private MeshLayerStates ReduceInitializeProject(MeshLayerStates previous, InitializeProjectIntent init)
    {
        var next = new MeshLayerStates();

        MeshData[] sortedMeshDatas = init.SaveData.MeshDatas;
        sortedMeshDatas.ToList().OrderBy(meshData => meshData.drawOrder).ToArray();

        foreach (MeshData meshData in sortedMeshDatas)
        {
            next.MeshLayers[meshData.ID] = new()
            {
                ID = meshData.ID,
                Name = meshData.name,
                Texture = meshData.texture.Data,
                SourcePath = meshData.sourcePath,
                MeshTransform = meshData.transform,
                AnimationTransform = new TransformData(),
                DrawOrder = meshData.drawOrder
            };
        }

        foreach (ParamCurve curve in init.SaveData.ParamCurves)
        {
            Parameter parameter = init.SaveData.Parameters.FirstOrDefault(p => p.ID == curve.ParamID);

            List<ParamPoint> paramPoints = init.SaveData.ParamPoints
            .Where(p => curve.ParamPoints.Contains(p.ID))
            .Select(p => p)
            .ToList();

            ParamPoint defaultPoint = paramPoints
            .Where(p => Math.Abs(parameter.DefaultValue - p.ParamValue) < 0.1f)
            .Select(p => p)
            .FirstOrDefault();

            next.MeshLayers[curve.MeshID].AnimationTransform = defaultPoint.transform.Clone();
        }

        return next;
    }

    public MeshLayerStates Update(MeshLayerStates previous, IModelContext context)
    {
        var next = new MeshLayerStates(context, previous);
        return next;
    }
}

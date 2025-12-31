using System;
using System.Linq;
using Assets.Scripts.States;
using Assets.Scripts.Utility.MVI;
using UnityEngine;

public class MeshReducer : IReducer<MeshStates>
{
    public MeshStates Reduce(MeshStates previous, IIntent intent)
    {
        return intent switch
        {
            SelectLayerIntent select => ReduceSelectLayer(previous, select),
            UpdateTransformIntent update => ReduceUpdateTransform(previous, update),
            SaveTransformIntent save => ReduceSaveTransform(previous, save),
            InterpolateTransformIntent interpolate => ReduceInterpolateTransform(previous, interpolate),
            ResetInterpolationIntent reset => ReduceResetInterpolation(previous, reset),
            CreateMeshLayerIntent create => ReduceCreateMeshLayer(previous, create),
            MoveLayerDownIntent _ => ReduceMoveLayerDown(previous),
            MoveLayerUpIntent _ => ReduceMoveLayerUp(previous),
            DeleteLayerIntent _ => ReduceDeleteLayer(previous),
            _ => previous
        };
    }

    private MeshStates ReduceDeleteLayer(MeshStates previous)
    {
        if (previous.SelectedMeshID == Guid.Empty) return previous;

        var next = previous.Clone();
        next.Meshes.Remove(previous.SelectedMeshID);
        next.SelectedMeshID = Guid.Empty;

        return next;
    }

    private MeshStates ReduceMoveLayerDown(MeshStates previous)
    {
        var next = previous.Clone();
        MeshState selectedMesh = next.Meshes[previous.SelectedMeshID];
        ushort inf = 0;
        Guid swapID = Guid.Empty;
        foreach ((_, MeshState meshState) in next.Meshes)
        {
            if (meshState.DrawOrder < selectedMesh.DrawOrder && meshState.DrawOrder >= inf)
            {
                inf = meshState.DrawOrder;
                swapID = meshState.ID;
            }
        }

        if (swapID != Guid.Empty)
        {
            MeshState swappedMesh = next.Meshes[swapID];
            (swappedMesh.DrawOrder, selectedMesh.DrawOrder) = (selectedMesh.DrawOrder, swappedMesh.DrawOrder);
        }
        else
            return previous;

        return next;
    }

    private MeshStates ReduceMoveLayerUp(MeshStates previous)
    {
        var next = previous.Clone();
        MeshState selectedMesh = next.Meshes[previous.SelectedMeshID];
        ushort inf = ushort.MaxValue;
        Guid swapID = Guid.Empty;
        foreach ((_, MeshState meshState) in next.Meshes)
        {
            if (meshState.DrawOrder > selectedMesh.DrawOrder && meshState.DrawOrder < inf)
            {
                inf = meshState.DrawOrder;
                swapID = meshState.ID;
            }
        }

        if (swapID != Guid.Empty)
        {
            MeshState swappedMesh = next.Meshes[swapID];
            (swappedMesh.DrawOrder, selectedMesh.DrawOrder) = (selectedMesh.DrawOrder, swappedMesh.DrawOrder);
        }
        else
            return previous;

        return next;
    }

    private MeshStates ReduceCreateMeshLayer(MeshStates previous, CreateMeshLayerIntent create)
    {
        var next = previous.Clone();

        next.Meshes[create.ID] = new()
        {
            ID = create.ID,
            MeshTransform = new TransformData() { Scale = Vector3.one },
            AnimationTransform = new TransformData(),
            DrawOrder = (ushort)next.Meshes.Count
        };

        return next;
    }

    private MeshStates ReduceSelectLayer(MeshStates previous, SelectLayerIntent select)
    {
        return new()
        {
            Meshes = previous.Meshes.ToDictionary(
                p => p.Key,
                p => new MeshState(p.Value)
                {
                    IsSelected = p.Key == select.LayerID
                }
            ),
            SelectedMeshID = select.LayerID
        };
    }

    private MeshStates ReduceResetInterpolation(MeshStates previous, ResetInterpolationIntent reset)
    {
        return new()
        {
            Meshes = previous.Meshes.ToDictionary(
                p => p.Key,
                p => new MeshState(p.Value)
                {
                    IsInterpolated = false
                }
            ),
            SelectedMeshID = previous.SelectedMeshID
        };
    }

    private MeshStates ReduceInterpolateTransform(MeshStates previous, InterpolateTransformIntent interpolate)
    {
        var next = previous.Clone();

        var mesh = next.Meshes[interpolate.MeshID];
        mesh.AnimationTransform = interpolate.Delta;
        mesh.InterpolatedTransform = mesh.MeshTransform + interpolate.Delta;
        mesh.IsInterpolated = true;

        return next;
    }

    private MeshStates ReduceSaveTransform(MeshStates previous, SaveTransformIntent save)
    {
        return previous;
    }

    private MeshStates ReduceUpdateTransform(MeshStates previous, UpdateTransformIntent update)
    {
        // TODO: move this to state and connect an intent to set to true
        bool areParametersAssigned = ParamCurveRegistry.Instance.GetAssignedParamIDsOfMesh(update.MeshID).Count > 0;

        var next = previous.Clone();
        var mesh = next.Meshes[update.MeshID];

        switch (update.Type)
        {
            case TransformType.POSITION:
                if (areParametersAssigned)
                    mesh.AnimationTransform.Position = update.Data.Position - mesh.MeshTransform.Position;
                else
                    mesh.MeshTransform.Position = update.Data.Position;
                break;
            case TransformType.ROTATION:
                if (areParametersAssigned)
                    mesh.AnimationTransform.Rotation = Quaternion.Inverse(mesh.MeshTransform.Rotation) * update.Data.Rotation;
                else
                    mesh.MeshTransform.Rotation = update.Data.Rotation;
                break;
            case TransformType.SCALE:
                if (areParametersAssigned)
                    mesh.AnimationTransform.Scale = update.Data.Scale - mesh.MeshTransform.Scale;
                else
                    mesh.MeshTransform.Scale = update.Data.Scale;
                break;
        }
        return next;
    }
}
using System;
using Assets.Scripts.States;
using Assets.Scripts.Utility.MVI;
using UnityEngine;

public class MeshReducer : IReducer<MeshState>
{
    public MeshState Reduce(MeshState previous, IIntent intent)
    {
        return intent switch
        {
            SelectLayerIntent select => ReduceSelectLayer(previous, select),
            UpdateTransformIntent update => ReduceUpdateTransform(previous, update),
            SaveTransformIntent save => ReduceSaveTransform(previous, save),
            InterpolateTransformIntent interpolate => ReduceInterpolateTransform(previous, interpolate),
            ResetInterpolationIntent reset => ReduceResetInterpolation(previous, reset),
            _ => previous
        };
    }

    private MeshState ReduceSelectLayer(MeshState previous, SelectLayerIntent select)
    {
        return new MeshState(previous)
        {
            IsSelected = previous.ID == select.LayerID
        };
    }

    private MeshState ReduceResetInterpolation(MeshState previous, ResetInterpolationIntent reset)
    {
        return new MeshState(previous)
        {
            IsInterpolated = false
        };
    }

    private MeshState ReduceInterpolateTransform(MeshState previous, InterpolateTransformIntent interpolate)
    {
        return new MeshState(previous)
        {
            AnimationTransform = interpolate.Delta,
            InterpolatedTransform = previous.MeshTransform + interpolate.Delta,
            IsInterpolated = true
        };
    }

    private MeshState ReduceSaveTransform(MeshState previous, SaveTransformIntent save)
    {
        return previous;
    }

    private MeshState ReduceUpdateTransform(MeshState previous, UpdateTransformIntent update)
    {
        // TODO: move this to state and connect an intent to set to true
        bool areParametersAssigned = ParamCurveRegistry.Instance.GetAssignedParamIDsOfMesh(previous.ID).Count > 0;

        var next = new MeshState(previous);

        switch (update.Type)
        {
            case TransformType.POSITION:
                if (areParametersAssigned)
                    next.AnimationTransform.Position = update.Data.Position - previous.MeshTransform.Position;
                else
                    next.MeshTransform.Position = update.Data.Position;
                break;
            case TransformType.ROTATION:
                if (areParametersAssigned)
                    next.AnimationTransform.Rotation = Quaternion.Inverse(previous.MeshTransform.Rotation) * update.Data.Rotation;
                else
                    next.MeshTransform.Rotation = update.Data.Rotation;
                break;
            case TransformType.SCALE:
                if (areParametersAssigned)
                    next.AnimationTransform.Scale = update.Data.Scale - previous.MeshTransform.Scale;
                else
                    next.MeshTransform.Scale = update.Data.Scale;
                break;
        }
        return next;
    }
}
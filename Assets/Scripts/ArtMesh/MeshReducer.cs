using System;
using Assets.Scripts.ArtMesh;
using Assets.Scripts.Utility.MVI;
using UnityEngine;

public class MeshReducer : IReducer<MeshState>
{
    public bool CanReduce(IIntent intent)
    {
        Type intentType = intent.GetType();
        return intentType == typeof(UpdateTransformIntent)
            || intentType == typeof(SaveTransformIntent)
            || intentType == typeof(InterpolateTransformIntent)
            || intentType == typeof(ResetInterpolationIntent)
        ;
    }

    public MeshState Reduce(MeshState previous, IIntent intent)
    {
        return intent switch
        {
            UpdateTransformIntent update => ReduceUpdateTransform(previous, update),
            SaveTransformIntent save => ReduceSaveTransform(previous, save),
            InterpolateTransformIntent interpolate => ReduceInterpolateTransform(previous, interpolate),
            ResetInterpolationIntent reset => ReduceResetInterpolation(previous, reset),
            _ => previous
        };
    }

    private MeshState ReduceResetInterpolation(MeshState previous, ResetInterpolationIntent reset)
    {
        return new MeshState()
        {
            ID = previous.ID,
            MeshTransform = previous.MeshTransform,
            AnimationTransform = previous.AnimationTransform,
            InterpolatedTransform = previous.InterpolatedTransform,
            IsInterpolated = false
        };
    }

    private MeshState ReduceInterpolateTransform(MeshState previous, InterpolateTransformIntent interpolate)
    {
        return new MeshState()
        {
            ID = previous.ID,
            MeshTransform = previous.MeshTransform,
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

        var next = new MeshState()
        {
            ID = previous.ID,
            MeshTransform = previous.MeshTransform,
            AnimationTransform = previous.AnimationTransform,
            InterpolatedTransform = previous.InterpolatedTransform,
            IsInterpolated = previous.IsInterpolated
        };

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
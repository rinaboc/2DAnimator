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
            UpdateTransformIntent update => ReduceUpdateTransform(previous, update),
            SaveTransformIntent save => ReduceUpdateTransform(previous, new UpdateTransformIntent(save.MeshID, save.Data, save.Type)),
            InterpolateTransformIntent interpolate => ReduceInterpolateTransform(previous, interpolate),
            ResetInterpolationIntent reset => ReduceResetInterpolation(previous, reset),
            // InitializeProjectIntent init => ReduceInitializeProject(previous, init),
            _ => previous
        };
    }

    private MeshLayerStates ReduceUpdateTransform(MeshLayerStates previous, UpdateTransformIntent update)
    {
        bool areParametersAssigned = previous.MeshLayers[update.MeshID].HasParametersAssigned;

        var next = previous.Clone();
        var mesh = next.MeshLayers[update.MeshID];

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

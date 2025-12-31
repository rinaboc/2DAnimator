using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.States;
using Assets.Scripts.Utility.MVI;
using UnityEngine;

public class MeshCommandHandler : ICommandHandler
{
    public void Execute(IIntent intent, object state, IModelContext context)
    {
        switch (intent)
        {
            case SaveTransformIntent save:
                ExecuteSaveTransform(save, state, context);
                break;
            case CreateMeshLayerIntent create:
                ExecuteCreateMeshLayer(create, state, context);
                break;
            case MoveLayerUpIntent _:
                ExecuteMoveLayerUp(state, context);
                break;
            case MoveLayerDownIntent _:
                ExecuteMoveLayerDown(state, context);
                break;
            case DeleteLayerIntent _:
                ExecuteDeleteLayer(state, context);
                break;
        }
        ;
    }

    private void ExecuteDeleteLayer(object state, IModelContext context)
    {
        context.Meshes.Remove(context.GeneralSettings.SelectedMeshID);
        MeshManager.Instance.DeleteArtMeshObj(context.GeneralSettings.SelectedMeshID);

        List<ParamCurve> paramCurves = context.ParamCurves.GetParamCurvesOfMesh(context.GeneralSettings.SelectedMeshID);
        for (int i = 0; i < paramCurves.Count; i++)
        {
            ParamCurve paramCurve = paramCurves[i];
            foreach (Guid pointID in paramCurve.ParamPoints)
            {
                context.ParamPoints.Remove(pointID);
            }
            context.ParamCurves.Remove(paramCurve.ID);
        }
    }

    private void ExecuteMoveLayerDown(object state, IModelContext context)
    {
        context.Meshes.TryGet(context.GeneralSettings.SelectedMeshID, out MeshData meshData);
        if (!((MeshRegistry)context.Meshes).TryGetNextDrawOrder(meshData.drawOrder, out MeshData next)) return;
        (next.drawOrder, meshData.drawOrder) = (meshData.drawOrder, next.drawOrder);
    }

    private void ExecuteMoveLayerUp(object state, IModelContext context)
    {
        context.Meshes.TryGet(context.GeneralSettings.SelectedMeshID, out MeshData meshData);
        if (!((MeshRegistry)context.Meshes).TryGetPreviousDrawOrder(meshData.drawOrder, out MeshData previous)) return;
        (previous.drawOrder, meshData.drawOrder) = (meshData.drawOrder, previous.drawOrder);
    }

    private void ExecuteCreateMeshLayer(CreateMeshLayerIntent create, object state, IModelContext context)
    {
        MeshData newMesh = new(create.Path, autoRegister: false)
        {
            ID = create.ID
        };

        context.Meshes.Register(newMesh);
        MeshManager.Instance.CreateArtMeshObj(create.Tex, create.ID);
    }

    private void ExecuteSaveTransform(SaveTransformIntent save, object state, IModelContext context)
    {
        var mesh = ((MeshStates)state).Meshes[save.MeshID];
        bool areParametersAssigned = context.ParamCurves.GetAssignedParamIDsOfMesh(mesh.ID).Count > 0;

        if (areParametersAssigned)
        {
            if (!context.Parameters.TryGet(context.GeneralSettings.SelectedParamID, out Parameter currentParam)) return;
            List<ParamCurve> currentParamCurves = context.ParamCurves.GetEntries(currentParam.ParamCurves);

            float sliderValue = ParameterManager.Instance.GetParamSlider(currentParam.ID).GetValue();
            for (int i = 0; i < currentParamCurves.Count; i++)
            {
                ParamCurve paramCurve = currentParamCurves[i];

                if (paramCurve.MeshID != mesh.ID) // filter by mesh id
                    continue;

                List<ParamPoint> paramPoints = context.ParamPoints.GetEntries(paramCurve.ParamPoints);

                bool isPointUpdated = false;
                float[] distFromPointValues = new float[paramPoints.Count];
                for (int j = 0; j < paramPoints.Count; j++)
                {
                    ParamPoint point = paramPoints[j];
                    distFromPointValues[j] = point.Dist(sliderValue);

                    if (distFromPointValues[j] > 0.01f)
                    {
                        continue;
                    }

                    isPointUpdated = true;
                    switch (save.Type)
                    {
                        case TransformType.POSITION:
                            point.transform.Position = mesh.AnimationTransform.Position;
                            break;
                        case TransformType.ROTATION:
                            point.transform.Rotation = mesh.AnimationTransform.Rotation;
                            break;
                        case TransformType.SCALE:
                            point.transform.Scale = mesh.AnimationTransform.Scale;
                            break;
                    }

                    Debug.Log($"updated point at {sliderValue}: " + point);
                    break;
                }
                if (!isPointUpdated)
                {
                    Debug.Log("no point was updated");
                    int minIndex = Array.IndexOf(distFromPointValues, distFromPointValues.Min());
                    ParameterManager.Instance.GetParamSlider(paramCurve.ParamID).SetValue(paramPoints[minIndex].ParamValue);
                    ParameterManager.Instance.DispatchToParameterStore(new InterpolateParameterIntent(paramCurve.ParamID, paramPoints[minIndex].ParamValue));
                }
            }
        }
    }
}


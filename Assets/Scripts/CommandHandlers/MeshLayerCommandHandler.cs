using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.States;
using Assets.Scripts.Utility.MVI;
using UnityEngine;

public class MeshLayerCommandHandler : ICommandHandler
{
    public void Execute(IIntent intent, object state, IModelContext context)
    {
        var meshLayerState = state as MeshLayerStates;

        switch (intent)
        {
            case ChangeLayerNameIntent _: ExecuteChangeLayerName(meshLayerState, context); break;
            case CreateMeshLayerIntent _: ExecuteLayerStateChanged(meshLayerState, context); break;
            case SelectLayerIntent _: ExecuteSelectLayer(meshLayerState, context); break;
            case MoveLayerUpIntent _: ExecuteLayerOrderChanged(meshLayerState, context); break;
            case MoveLayerDownIntent _: ExecuteLayerOrderChanged(meshLayerState, context); break;
            case DeleteLayerIntent _: ExecuteLayerStateChanged(meshLayerState, context); break;
            case InitializeProjectIntent init: ExecuteInitializeProject(init, context); break;
            case SaveTransformIntent _: ExecuteSaveTransform(meshLayerState, context); break;
        }
    }

    private void ExecuteSaveTransform(MeshLayerStates state, IModelContext context)
    {
        if (!context.Parameters.TryGet(context.GeneralSettings.SelectedParamID, out Parameter currentParam)) return;
        foreach ((var _, var mesh) in state.MeshLayers)
        {
            bool areParametersAssigned = context.ParamCurves.GetAssignedParamIDsOfMesh(mesh.ID).Count > 0;
            if (!areParametersAssigned) continue;

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

                    if (distFromPointValues[j] > 0.01f) continue;

                    isPointUpdated = true;
                    point.transform.Position = mesh.AnimationTransform.Position;
                    point.transform.Rotation = mesh.AnimationTransform.Rotation;
                    point.transform.Scale = mesh.AnimationTransform.Scale;

                    Debug.Log($"updated point at {sliderValue}: " + point);
                    break;
                }

                if (isPointUpdated) continue;
                Debug.Log("no point was updated");
                int minIndex = Array.IndexOf(distFromPointValues, distFromPointValues.Min());
                ParameterManager.Instance.GetParamSlider(paramCurve.ParamID).SetValue(paramPoints[minIndex].ParamValue);
                ParameterManager.Instance.DispatchToParameterStore(new InterpolateParameterIntent(paramCurve.ParamID, paramPoints[minIndex].ParamValue));
            }

        }
    }

    private void ExecuteInitializeProject(InitializeProjectIntent init, IModelContext context)
    {
        context.Meshes.Clear();
        MeshManager.Instance.ClearArtMeshObjects();

        LayerManager.Instance.DeleteAllUILayers();
        foreach (var item in init.SaveData.MeshDatas)
        {
            if (item.texture.Data == null)
            { Debug.LogError("couldn't load image texture"); continue; }
            context.Meshes.Register(item);
            MeshManager.Instance.CreateArtMeshObj(item.texture.Data, item.ID);

            LayerManager.Instance.CreateUIArtLayer(item.ID);
        }

        foreach (MeshData meshData in init.SaveData.MeshDatas)
        {
            LayerManager.Instance.SetSiblingIndex(meshData.ID, meshData.drawOrder);
        }
    }

    private void ExecuteLayerOrderChanged(MeshLayerStates state, IModelContext context)
    {
        foreach ((var _, var meshLayerState) in state.MeshLayers)
        {
            LayerManager.Instance.SetSiblingIndex(meshLayerState.ID, meshLayerState.DrawOrder);

            context.Meshes.TryGet(meshLayerState.ID, out MeshData meshData);
            meshData.drawOrder = (ushort)meshLayerState.DrawOrder;
        }
    }

    private void ExecuteLayerStateChanged(MeshLayerStates state, IModelContext context)
    {
        var layers = LayerManager.Instance.GetUILayerIDs();
        foreach ((var _, var meshLayerState) in state.MeshLayers)
        {
            if (!layers.Contains(meshLayerState.ID)) // create action
            {
                LayerManager.Instance.CreateUIArtLayer(meshLayerState.ID);
                LayerManager.Instance.SetSiblingIndex(meshLayerState.ID, meshLayerState.DrawOrder);

                MeshData newMesh = new(meshLayerState.Name)
                {
                    ID = meshLayerState.ID,
                    drawOrder = (ushort)meshLayerState.DrawOrder,
                    transform = meshLayerState.MeshTransform,
                    sourcePath = meshLayerState.SourcePath,
                    texture = new(meshLayerState.Texture),
                    name = meshLayerState.Name
                };

                context.Meshes.Register(newMesh);
                MeshManager.Instance.CreateArtMeshObj(meshLayerState.Texture, meshLayerState.ID);
            }
            else
            {
                layers.Remove(meshLayerState.ID);
                context.Meshes.TryGet(meshLayerState.ID, out MeshData meshData);
                meshData.drawOrder = (ushort)meshLayerState.DrawOrder;
                meshData.transform = meshLayerState.MeshTransform;
                meshData.sourcePath = meshLayerState.SourcePath;
                meshData.texture = new(meshLayerState.Texture);
                meshData.name = meshLayerState.Name;
            }
        }

        foreach (Guid id in layers) // delete action
        {
            LayerManager.Instance.DeleteUILayer(id);

            context.Meshes.Remove(id);
            MeshManager.Instance.DeleteArtMeshObj(id);

            List<ParamCurve> paramCurves = context.ParamCurves.GetParamCurvesOfMesh(id);
            for (int i = 0; i < paramCurves.Count; i++)
            {
                ParamCurve paramCurve = paramCurves[i];
                foreach (Guid pointID in paramCurve.ParamPoints)
                {
                    context.ParamPoints.Remove(pointID);
                }
                context.ParamCurves.Remove(paramCurve.ID);
            }

            if (context.GeneralSettings.SelectedMeshID == id)
            {
                ParameterManager.Instance.HighlightCurves(new());
            }
        }

        context.GeneralSettings.SelectedMeshID = state.SelectedMeshLayerID;
    }

    private void ExecuteSelectLayer(MeshLayerStates state, IModelContext context)
    {
        context.GeneralSettings.SelectedMeshID = state.SelectedMeshLayerID;

        List<Guid> paramIDs = context.ParamCurves.GetAssignedParamIDsOfMesh(state.SelectedMeshLayerID);
        ParameterManager.Instance.HighlightCurves(paramIDs);
    }

    private void ExecuteChangeLayerName(MeshLayerStates state, IModelContext context)
    {
        foreach ((var id, var meshLayerState) in state.MeshLayers)
        {
            context.Meshes.TryGet(id, out MeshData meshData);
            meshData.name = meshLayerState.Name;
        }
    }
}

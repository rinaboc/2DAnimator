using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Utility.MVI;
using UnityEngine;

public class MeshLayerCommandHandler : ICommandHandler
{
    public Action Execute(IIntent intent, IModelContext context)
    {
        return intent switch
        {
            ChangeLayerNameIntent change => ExecuteChangeLayerName(change, context),
            CreateMeshLayerIntent create => ExecuteCreateMeshLayer(create, context),
            SelectLayerIntent select => ExecuteSelectLayer(select, context),
            MoveLayerUpIntent _ => ExecuteMoveLayer(context, true),
            MoveLayerDownIntent _ => ExecuteMoveLayer(context, false),
            DeleteLayerIntent _ => ExecuteDeleteLayer(context),
            ResetInterpolationIntent _ => ExecuteResetInterpolation(context),
            SaveTransformIntent save => ExecuteSaveTransform(save, context),
            InitializeProjectIntent init => ExecuteInitializeProject(init, context),
            _ => null
        };
    }

    private Action ExecuteResetInterpolation(IModelContext context)
    {
        if (!context.Meshes.TryGet(context.GeneralSettings.SelectedMeshID, out MeshData mesh)) return null;
        bool areParametersAssigned = context.ParamCurves.GetAssignedParamIDsOfMesh(mesh.ID).Count > 0;
        if (areParametersAssigned) return null; // TODO: need to check for param points

        var previousTransform = mesh.transform.Clone();
        return () =>
        {
            context.Meshes.TryGet(mesh.ID, out MeshData m);
            m.transform = previousTransform.Clone();
        };
    }

    private Action ExecuteSaveTransform(SaveTransformIntent save, IModelContext context)
    {
        bool areParametersAssigned = context.ParamCurves.GetAssignedParamIDsOfMesh(save.MeshID).Count > 0;
        context.Meshes.TryGet(save.MeshID, out MeshData mesh);

        if (!areParametersAssigned)
        {
            var previousTransform = mesh.transform.Clone();
            switch (save.Type)
            {
                case TransformType.POSITION:
                    mesh.transform.Position = save.Data.Position;
                    break;
                case TransformType.ROTATION:
                    mesh.transform.Rotation = save.Data.Rotation;
                    break;
                case TransformType.SCALE:
                    mesh.transform.Scale = save.Data.Scale;
                    break;
            }
            return () =>
            {
                context.Meshes.TryGet(mesh.ID, out MeshData m);
                m.transform = previousTransform;
            };
        }

        if (!context.Parameters.TryGet(context.GeneralSettings.SelectedParamID, out Parameter currentParam)) return null;
        List<ParamCurve> currentParamCurves = context.ParamCurves.GetEntries(currentParam.ParamCurves);

        float sliderValue = ParameterManager.Instance.GetParamSlider(currentParam.ID).GetValue();
        ParamPoint pointBeforeUpdate = null;
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
                pointBeforeUpdate = point.Copy();

                switch (save.Type)
                {
                    case TransformType.POSITION:
                        point.transform.Position = save.Data.Position - mesh.transform.Position;
                        break;
                    case TransformType.ROTATION:
                        point.transform.Rotation = Quaternion.Inverse(mesh.transform.Rotation) * save.Data.Rotation;
                        break;
                    case TransformType.SCALE:
                        point.transform.Scale = save.Data.Scale - mesh.transform.Scale;
                        break;
                }

                break;
            }

            int minIndex = Array.IndexOf(distFromPointValues, distFromPointValues.Min());
            ParameterManager.Instance.GetParamSlider(paramCurve.ParamID).SetValue(paramPoints[minIndex].ParamValue);

            if (isPointUpdated) continue;
            Debug.Log("no point was updated on this curve");
            AnimationManager.Instance.InterpolateParameter(paramPoints[minIndex].ParamValue, paramCurve.ParamID, context);
        }


        return () =>
        {
            // TODO: check if this works correctly
            if (pointBeforeUpdate != null)
            {
                context.ParamPoints.TryGet(pointBeforeUpdate.ID, out ParamPoint point);
                point.transform = pointBeforeUpdate.transform.Clone();
            }
        };
    }

    private Action ExecuteDeleteLayer(IModelContext context)
    {
        if (!context.Meshes.TryGet(context.GeneralSettings.SelectedMeshID, out MeshData mesh)) return null;

        LayerManager.Instance.DeleteUILayer(mesh.ID);

        context.Meshes.Remove(mesh.ID);
        MeshManager.Instance.DeleteArtMeshObj(mesh.ID);

        List<ParamCurve> deletedParamCurves = new();
        List<ParamPoint> deletedParamPoints = new();

        List<ParamCurve> paramCurves = context.ParamCurves.GetParamCurvesOfMesh(mesh.ID);
        for (int i = 0; i < paramCurves.Count; i++)
        {
            ParamCurve paramCurve = paramCurves[i];
            foreach (Guid pointID in paramCurve.ParamPoints)
            {
                context.ParamPoints.TryGet(pointID, out ParamPoint point);
                deletedParamPoints.Add(point);

                context.ParamPoints.Remove(pointID);
            }
            deletedParamCurves.Add(paramCurve);
            context.ParamCurves.Remove(paramCurve.ID);
        }

        ParameterManager.Instance.HighlightCurves(new());
        context.GeneralSettings.SelectedMeshID = Guid.Empty;

        // TODO: check if it works without this reordering
        var orderedLayers = context.Meshes.GetAll()
            .OrderBy(layer => layer.drawOrder)
            .ToList();
        for (int i = 0; i < orderedLayers.Count; i++)
        {
            orderedLayers[i].drawOrder = (ushort)i;
        }

        return () =>
        {
            LayerManager.Instance.CreateUIArtLayer(mesh.ID);
            LayerManager.Instance.SetSiblingIndex(mesh.ID, mesh.drawOrder);

            context.Meshes.Register(mesh);
            MeshManager.Instance.CreateArtMeshObj(mesh.texture.Data, mesh.ID);

            foreach (var pc in deletedParamCurves) context.ParamCurves.Register(pc);
            foreach (var pp in deletedParamPoints) context.ParamPoints.Register(pp);

            context.GeneralSettings.SelectedMeshID = mesh.ID;
            ParameterManager.Instance.HighlightCurves(context.ParamCurves.GetAssignedParamIDsOfMesh(mesh.ID));
        };
    }

    private Action ExecuteMoveLayer(IModelContext context, bool up)
    {
        if (!context.Meshes.TryGet(context.GeneralSettings.SelectedMeshID, out MeshData curMesh)) return null;

        ushort inf = up ? (ushort)0 : ushort.MaxValue;
        Guid swapID = Guid.Empty;
        MeshData swappedMesh = null;
        foreach (MeshData meshData in context.Meshes.GetAll())
            if ((up && meshData.drawOrder < curMesh.drawOrder && meshData.drawOrder >= inf) ||
                (!up && meshData.drawOrder > curMesh.drawOrder && meshData.drawOrder < inf))
            {
                inf = meshData.drawOrder;
                swapID = meshData.ID;
                swappedMesh = meshData;
            }

        if (swapID != Guid.Empty)
        {
            (swappedMesh.drawOrder, curMesh.drawOrder) = (curMesh.drawOrder, swappedMesh.drawOrder);
            LayerManager.Instance.SetSiblingIndex(curMesh.ID, curMesh.drawOrder);
            LayerManager.Instance.SetSiblingIndex(swappedMesh.ID, swappedMesh.drawOrder);
        }


        return () =>
        {
            if (swapID == Guid.Empty) return;

            (swappedMesh.drawOrder, curMesh.drawOrder) = (curMesh.drawOrder, swappedMesh.drawOrder);

            LayerManager.Instance.SetSiblingIndex(curMesh.ID, curMesh.drawOrder);
            LayerManager.Instance.SetSiblingIndex(swappedMesh.ID, swappedMesh.drawOrder);
        };

    }

    private Action ExecuteSelectLayer(SelectLayerIntent select, IModelContext context)
    {
        Guid prevID = context.GeneralSettings.SelectedMeshID;

        context.GeneralSettings.SelectedMeshID = select.LayerID;

        List<Guid> paramIDs = context.ParamCurves.GetAssignedParamIDsOfMesh(select.LayerID);
        ParameterManager.Instance.HighlightCurves(paramIDs);

        return () =>
        {
            if (prevID != Guid.Empty)
            {
                List<Guid> prevParamIDs = context.ParamCurves.GetAssignedParamIDsOfMesh(prevID);
                ParameterManager.Instance.HighlightCurves(prevParamIDs);
            }
            else
                ParameterManager.Instance.HighlightCurves(new());

            context.GeneralSettings.SelectedMeshID = prevID;
        };
    }

    private Action ExecuteCreateMeshLayer(CreateMeshLayerIntent create, IModelContext context)
    {
        MeshData newMesh = new(create.Path)
        {
            ID = create.ID,
            drawOrder = (ushort)context.Meshes.Count(),
            transform = new TransformData() { Scale = Vector3.one },
            texture = new(create.Tex),
            name = "Layer " + context.Meshes.Count()
        };

        LayerManager.Instance.CreateUIArtLayer(newMesh.ID);
        LayerManager.Instance.SetSiblingIndex(newMesh.ID, newMesh.drawOrder);

        context.Meshes.Register(newMesh);
        MeshManager.Instance.CreateArtMeshObj(newMesh.texture.Data, newMesh.ID);

        return () =>
        {
            context.Meshes.Remove(newMesh.ID);
            LayerManager.Instance.DeleteUILayer(newMesh.ID);
            MeshManager.Instance.DeleteArtMeshObj(newMesh.ID);
        };
    }

    private Action ExecuteChangeLayerName(ChangeLayerNameIntent change, IModelContext context)
    {
        context.Meshes.TryGet(change.LayerID, out MeshData meshData);
        string oldName = meshData.name;
        meshData.name = change.NewName;

        return () => meshData.name = oldName;
    }
    private Action ExecuteInitializeProject(InitializeProjectIntent init, IModelContext context)
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

        return null;
    }
}

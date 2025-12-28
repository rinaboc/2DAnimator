using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.ArtMesh;
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
        }
        ;
    }

    private void ExecuteSaveTransform(SaveTransformIntent save, object state, IModelContext context)
    {
        var previous = (MeshState)state;
        bool areParametersAssigned = context.ParamCurves.GetAssignedParamIDsOfMesh(previous.ID).Count > 0;

        if (areParametersAssigned)
        {
            if (!context.Parameters.TryGet(context.GeneralSettings.SelectedParamID, out Parameter currentParam)) return;
            List<ParamCurve> currentParamCurves = context.ParamCurves.GetEntries(currentParam.ParamCurves);

            float sliderValue = ParameterManager.Instance.GetParamSlider(currentParam.ID).GetValue();
            for (int i = 0; i < currentParamCurves.Count; i++)
            {
                ParamCurve paramCurve = currentParamCurves[i];

                if (paramCurve.MeshID != previous.ID) // filter by mesh id
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
                            point.transform.Position = previous.AnimationTransform.Position;
                            break;
                        case TransformType.ROTATION:
                            point.transform.Rotation = previous.AnimationTransform.Rotation;
                            break;
                        case TransformType.SCALE:
                            point.transform.Scale = previous.AnimationTransform.Scale;
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
                    ParameterManager.Instance.DispatchToParameterStore(paramCurve.ParamID, new InterpolateParameterIntent(paramCurve.ParamID, paramPoints[minIndex].ParamValue));
                }
            }
        }
    }
}


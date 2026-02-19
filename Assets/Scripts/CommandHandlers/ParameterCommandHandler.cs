using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.States;
using Assets.Scripts.Utility.MVI;

public class ParameterCommandHandler : ICommandHandler
{
    public void Execute(IIntent intent, object state, IModelContext context)
    {
        var parameterStates = (state as ParameterTimelineState)?.Parameters;

        switch (intent)
        {
            case SelectParameterIntent _: ExecuteSelectParameter(parameterStates, context); break;
            case CreateParameterIntent _: ExecuteParameterStateChanged(parameterStates, context); break;
            case CreateParamPointsIntent _: ExecuteCreateParamPoints(parameterStates, context); break;
            case DeleteSelectedParameterIntent _: ExecuteParameterStateChanged(parameterStates, context); break;
            case UpdateParameterIntent _: ExecuteParameterStateChanged(parameterStates, context); break;
            case OpenParameterCreatorIntent _: ExecuteOpenParameterCreator(parameterStates, context); break;
            case InitializeProjectIntent init: ExecuteInitializeProject(init, context); break;
            case InterpolateParameterIntent interpolate: ExecuteInterpolateParameter(interpolate, context); break;
            case TimelineParameterSliderChangedIntent slider: ExecuteTimelineParameterSliderChanged(slider, context); break;
        }
    }

    private void ExecuteTimelineParameterSliderChanged(TimelineParameterSliderChangedIntent slider, IModelContext context)
    {
        AnimationManager.Instance.InterpolateParameter(slider.Value, slider.ParamID, context);
    }

    private void ExecuteInterpolateParameter(InterpolateParameterIntent interpolate, IModelContext context)
    {
        AnimationManager.Instance.InterpolateParameter(interpolate.Value, interpolate.ParamID, context);
    }

    private void ExecuteInitializeProject(InitializeProjectIntent init, IModelContext context)
    {
        context.Parameters.Clear();
        context.ParamCurves.Clear();
        context.ParamPoints.Clear();

        ParameterManager.Instance.ClearParamSliders();
        foreach (Parameter parameter in init.SaveData.Parameters)
        {
            context.Parameters.Register(parameter);
            ParameterManager.Instance.CreateParameterSlider(parameter.ID);
        }

        foreach (ParamPoint point in init.SaveData.ParamPoints)
        {
            context.ParamPoints.Register(point);
        }

        foreach (ParamCurve curve in init.SaveData.ParamCurves)
        {
            context.ParamCurves.Register(curve);
            List<float> paramValues = new();
            foreach (var pointID in curve.ParamPoints)
            {
                if (context.ParamPoints.TryGet(pointID, out ParamPoint point))
                {
                    paramValues.Add(point.ParamValue);
                }
            }
            ParameterManager.Instance.GetParamSlider(curve.ParamID).CreateParamPointHandles(paramValues);
        }
    }

    private void ExecuteOpenParameterCreator(ParameterStates state, IModelContext context)
    {
        if (state == null) return;
        context.GeneralSettings.SelectedParamID = state.SelectedParamID;
    }

    private void ExecuteParameterStateChanged(ParameterStates state, IModelContext context)
    {
        if (state == null) return;
        var parameters = ParameterManager.Instance.GetParamSliderIDs();
        foreach ((var id, var parameterState) in state.Parameters)
        {
            if (!parameters.Contains(id)) // create action
            {
                Parameter parameter = new(parameterState.MinValue, parameterState.MaxValue,
                    parameterState.DefaultValue, parameterState.Name)
                {
                    ID = id
                };

                context.Parameters.Register(parameter);
                ParameterManager.Instance.CreateParameterSlider(parameterState.ID);
            }
            else // update action
            {
                parameters.Remove(id);

                context.Parameters.TryGet(id, out Parameter parameter);
                parameter.MinValue = parameterState.MinValue;
                parameter.MaxValue = parameterState.MaxValue;
                parameter.DefaultValue = parameterState.DefaultValue;
                parameter.Name = parameterState.Name;
            }
        }

        foreach (Guid id in parameters) // delete action
        {
            ParameterManager.Instance.DeleteParameterSlider(id);
            context.Parameters.Remove(id);
        }

        context.GeneralSettings.SelectedParamID = state.SelectedParamID;
    }

    private void ExecuteCreateParamPoints(ParameterStates state, IModelContext context)
    {
        if (state == null) return;
        var curves = context.ParamCurves.GetAll().ToList();
        foreach ((var id, var parameterState) in state.Parameters)
        {
            foreach (var meshID in parameterState.LinkedMeshLayers)
                if (!curves.Any(c => c.MeshID == meshID && c.ParamID == id)) // create action
                {
                    ParamCurve paramCurve = new(meshID, id);
                    context.ParamCurves.Register(paramCurve);

                    foreach (var value in parameterState.ParamPointValues)
                    {
                        ParamPoint point = new(value);
                        context.ParamPoints.Register(point);
                        paramCurve.ParamPoints.Add(point.ID);
                    }

                    context.Parameters.TryGet(id, out Parameter param);
                    param.ParamCurves.Add(paramCurve.ID);

                    ParameterManager.Instance.GetParamSlider(id).CreateParamPointHandles(parameterState.ParamPointValues);
                }
                else // update action
                {
                    curves.Remove(curves.FirstOrDefault(c => c.MeshID == meshID && c.ParamID == id));
                }
        }

        foreach (var curve in curves) // delete action
        {
            context.ParamCurves.Remove(curve.ID);
            foreach (var pointID in curve.ParamPoints)
                context.ParamPoints.Remove(pointID);
        }
        if (state.SelectedMeshLayerID == Guid.Empty) return;

        List<Guid> assignedParams = context.ParamCurves.GetAssignedParamIDsOfMesh(context.GeneralSettings.SelectedMeshID);
        ParameterManager.Instance.HighlightCurves(assignedParams);
    }

    private void ExecuteSelectParameter(ParameterStates state, IModelContext context)
    {
        if (state == null) return;
        context.GeneralSettings.SelectedParamID = state.SelectedParamID;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.States;
using Assets.Scripts.Utility.MVI;

public class ParameterReducer : IReducer<ParameterStates>
{
    public ParameterStates Reduce(ParameterStates previous, IIntent intent)
    {
        return intent switch
        {
            CloseParameterSettingsIntent _ => ReduceCloseParameterSettings(previous),
            OpenParameterCreatorIntent _ => ReduceOpenParameterCreator(previous),
            OpenParameterEditorIntent _ => ReduceOpenParameterEditor(previous),
            InterpolateParameterIntent interpolate => ReduceInterpolateParameter(previous, interpolate),
            ParameterValueInterpolatedIntent interpolate => ReduceParameterValueInterpolated(previous, interpolate),
            TimelineParameterSliderChangedIntent slider => ReduceTimelineParameterSliderChanged(previous, slider),
            // InitializeProjectIntent init => ReduceInitializeProject(previous, init),
            _ => previous
        };
    }

    private ParameterStates ReduceInterpolateParameter(ParameterStates previous, InterpolateParameterIntent interpolate)
    {
        var next = previous.Clone();
        next.Parameters[interpolate.ParamID].CurValue = (float)Math.Round(interpolate.Value, 2);
        return next;
    }

    private ParameterStates ReduceTimelineParameterSliderChanged(ParameterStates previous, TimelineParameterSliderChangedIntent slider)
    {
        var next = previous.Clone();
        next.Parameters[slider.ParamID].CurValue = slider.Value;
        return next;
    }

    private ParameterStates ReduceInitializeProject(ParameterStates previous, InitializeProjectIntent init)
    {
        ParameterStates next = new();

        foreach (Parameter parameter in init.SaveData.Parameters)
        {
            next.Parameters.Add(parameter.ID, new ParameterState()
            {
                ID = parameter.ID,
                MinValue = parameter.MinValue,
                MaxValue = parameter.MaxValue,
                DefaultValue = parameter.DefaultValue,
                Name = parameter.Name,
                IsSelected = false,
                CurValue = parameter.DefaultValue,
                ParamPointValues = { parameter.DefaultValue },
                LinkedMeshLayers = new()
            });
        }

        foreach (ParamCurve curve in init.SaveData.ParamCurves)
        {
            next.Parameters[curve.ParamID].ParamPointValues = init.SaveData.ParamPoints
            .Where(p => curve.ParamPoints.Contains(p.ID))
            .Select(p => p.ParamValue)
            .ToList();

            next.Parameters[curve.ParamID].LinkedMeshLayers.Add(curve.MeshID);
        }

        return next;
    }

    private ParameterStates ReduceParameterValueInterpolated(ParameterStates previous, ParameterValueInterpolatedIntent interpolate)
    {
        var ret = previous.Clone();
        foreach (KeyValuePair<Guid, float> paramValue in interpolate.ParamValues)
            ret.Parameters[paramValue.Key].CurValue = paramValue.Value;
        return ret;
    }

    private ParameterStates ReduceOpenParameterEditor(ParameterStates previous)
    {
        var ret = previous.Clone();
        ret.IsSettingsOpen = true;
        return ret;
    }

    private ParameterStates ReduceOpenParameterCreator(ParameterStates previous)
    {
        var next = previous.Clone();
        next.IsSettingsOpen = true;
        return next;
    }

    private ParameterStates ReduceCloseParameterSettings(ParameterStates previous)
    {
        var next = previous.Clone();
        next.IsSettingsOpen = false;
        return next;
    }

    public ParameterStates Update(ParameterStates previous, IModelContext context)
    {
        var next = new ParameterStates(context, previous);
        return next;
    }
}
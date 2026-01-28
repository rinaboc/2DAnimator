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
            InterpolateParameterIntent interpolate => ReduceInterpolateParameter(previous, interpolate),
            SelectParameterIntent select => ReduceSelectParameter(previous, select),
            CreateParameterIntent create => ReduceCreateParameter(previous, create),
            DeleteSelectedParameterIntent _ => ReduceDeleteSelectedParameter(previous),
            UpdateParameterIntent update => ReduceUpdateParameter(previous, update),
            CloseParameterSettingsIntent _ => ReduceCloseParameterSettings(previous),
            OpenParameterCreatorIntent _ => ReduceOpenParameterCreator(previous),
            OpenParameterEditorIntent _ => ReduceOpenParameterEditor(previous),
            CreateParamPointsIntent _ => ReduceCreateParamPoints(previous),
            ParameterValueInterpolatedIntent interpolate => ReduceParameterValueInterpolated(previous, interpolate),
            InitializeProjectIntent init => ReduceInitializeProject(previous, init),
            _ => previous
        };
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
                ParamPointValues = { parameter.DefaultValue }
            });
        }

        foreach (ParamCurve curve in init.SaveData.ParamCurves)
        {
            next.Parameters[curve.ParamID].ParamPointValues = init.SaveData.ParamPoints
            .Where(p => curve.ParamPoints.Contains(p.ID))
            .Select(p => p.ParamValue)
            .ToList();
        }

        return next;
    }

    private List<float> GetParamPointValues(float min, float max, float def)
    {
        List<float> ret = new() { min, max };

        if (Math.Abs(min - def) > 0.1f && Math.Abs(max - def) > 0.1f)
            ret.Add(def);

        return ret;
    }

    private ParameterStates ReduceCreateParamPoints(ParameterStates previous)
    {
        var ret = previous.Clone();
        var selectedParam = ret.Parameters[ret.SelectedParamID];
        List<float> paramValues = GetParamPointValues(selectedParam.MinValue, selectedParam.MaxValue, selectedParam.CurValue);

        selectedParam.ParamPointValues = paramValues;
        return ret;
    }

    private ParameterStates ReduceParameterValueInterpolated(ParameterStates previous, ParameterValueInterpolatedIntent interpolate)
    {
        var ret = previous.Clone();
        ret.Parameters[interpolate.ParamID].CurValue = interpolate.Value;
        return ret;
    }

    private ParameterStates ReduceOpenParameterEditor(ParameterStates previous)
    {
        return new ParameterStates
        {
            Parameters = previous.Parameters.ToDictionary(
                p => p.Key,
                p => new ParameterState(p.Value)
            ),
            SelectedParamID = previous.SelectedParamID,
            IsSettingsOpen = true
        };
    }

    private ParameterStates ReduceOpenParameterCreator(ParameterStates previous)
    {
        return new ParameterStates
        {
            Parameters = previous.Parameters.ToDictionary(
                p => p.Key,
                p => new ParameterState(p.Value)
                {
                    IsSelected = false
                }
            ),
            SelectedParamID = Guid.Empty,
            IsSettingsOpen = true
        };
    }

    private ParameterStates ReduceCloseParameterSettings(ParameterStates previous)
    {
        return new ParameterStates
        {
            Parameters = previous.Parameters.ToDictionary(
                p => p.Key,
                p => new ParameterState(p.Value)
            ),
            SelectedParamID = previous.SelectedParamID,
            IsSettingsOpen = false
        };
    }

    private ParameterStates ReduceUpdateParameter(ParameterStates previous, UpdateParameterIntent update)
    {
        var next = previous.Clone();

        var updatedParam = next.Parameters[update.ParamID];
        updatedParam.MinValue = update.Min;
        updatedParam.MaxValue = update.Max;
        updatedParam.DefaultValue = update.Default;
        updatedParam.Name = update.Name;
        updatedParam.CurValue = update.Default;

        return next;

    }

    private ParameterStates ReduceDeleteSelectedParameter(ParameterStates previous)
    {
        if (previous.SelectedParamID == Guid.Empty) return previous;

        var next = previous.Clone();

        next.Parameters.Remove(previous.SelectedParamID);
        next.SelectedParamID = Guid.Empty;

        return next;
    }

    private ParameterStates ReduceCreateParameter(ParameterStates previous, CreateParameterIntent create)
    {
        var next = previous.Clone();

        next.Parameters.Add(create.ParamID, new ParameterState()
        {
            ID = create.ParamID,
            MinValue = create.Min,
            MaxValue = create.Max,
            DefaultValue = create.Default,
            Name = create.Name,
            IsSelected = false,
            CurValue = create.Default,
            ParamPointValues = new() { create.Default }
        });

        return next;
    }

    private ParameterStates ReduceSelectParameter(ParameterStates previous, SelectParameterIntent select)
    {
        if (previous.SelectedParamID == select.ParamID) return previous;

        return new ParameterStates
        {
            Parameters = previous.Parameters.ToDictionary(
                p => p.Key,
                p => new ParameterState(p.Value)
                {
                    IsSelected = p.Key == select.ParamID
                }
            ),
            SelectedParamID = select.ParamID,
            IsSettingsOpen = previous.IsSettingsOpen
        };
    }

    private ParameterStates ReduceInterpolateParameter(ParameterStates previous, InterpolateParameterIntent interpolate)
    {
        AnimationManager.Instance.InterpolateParameter(interpolate.Value, interpolate.ParamID);
        return previous;
    }
}
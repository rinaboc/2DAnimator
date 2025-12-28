using System;
using System.Linq;
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
            _ => previous
        };
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
            IsSelected = false
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
using System;
using System.Linq;
using Assets.Scripts.Utility.MVI;

public class ParameterReducer : IReducer<ParameterStates>
{
    public bool CanReduce(IIntent intent)
    {
        Type intentType = intent.GetType();
        return intentType == typeof(InterpolateParameterIntent)
            || intentType == typeof(SelectParameterIntent)
            || intentType == typeof(CreateParameterIntent)
            || intentType == typeof(DeleteSelectedParameterIntent)
        ;
    }

    public ParameterStates Reduce(ParameterStates previous, IIntent intent)
    {
        return intent switch
        {
            InterpolateParameterIntent interpolate => ReduceInterpolateParameter(previous, interpolate),
            SelectParameterIntent select => ReduceSelectParameter(previous, select),
            CreateParameterIntent create => ReduceCreateParameter(previous, create),
            DeleteSelectedParameterIntent _ => ReduceDeleteSelectedParameter(previous),
            _ => previous
        };
    }

    private ParameterStates ReduceDeleteSelectedParameter(ParameterStates previous)
    {
        if (previous.SelectedParamID == Guid.Empty) return previous;

        var ret = new ParameterStates
        {
            Parameters = previous.Parameters.ToDictionary(
                p => p.Key,
                p => new ParameterStates.ParameterState()
                {
                    ID = p.Value.ID,
                    MinValue = p.Value.MinValue,
                    MaxValue = p.Value.MaxValue,
                    DefaultValue = p.Value.DefaultValue,
                    Name = p.Value.Name,
                    IsSelected = p.Value.IsSelected
                }
            ),
            SelectedParamID = previous.SelectedParamID
        };

        ret.Parameters.Remove(previous.SelectedParamID);
        ret.SelectedParamID = Guid.Empty;

        return ret;
    }

    private ParameterStates ReduceCreateParameter(ParameterStates previous, CreateParameterIntent create)
    {
        var ret = new ParameterStates
        {
            Parameters = previous.Parameters.ToDictionary(
                p => p.Key,
                p => new ParameterStates.ParameterState()
                {
                    ID = p.Value.ID,
                    MinValue = p.Value.MinValue,
                    MaxValue = p.Value.MaxValue,
                    DefaultValue = p.Value.DefaultValue,
                    Name = p.Value.Name,
                    IsSelected = p.Value.IsSelected
                }
            ),
            SelectedParamID = previous.SelectedParamID
        };

        ret.Parameters.Add(create.ParamID, new ParameterStates.ParameterState()
        {
            ID = create.ParamID,
            MinValue = create.Min,
            MaxValue = create.Max,
            DefaultValue = create.Default,
            Name = create.Name,
            IsSelected = false
        });

        return ret;
    }

    private ParameterStates ReduceSelectParameter(ParameterStates previous, SelectParameterIntent select)
    {
        if (previous.SelectedParamID == select.ParamID) return previous;

        return new ParameterStates
        {
            Parameters = previous.Parameters.ToDictionary(
                p => p.Key,
                p => new ParameterStates.ParameterState()
                {
                    ID = p.Value.ID,
                    MinValue = p.Value.MinValue,
                    MaxValue = p.Value.MaxValue,
                    DefaultValue = p.Value.DefaultValue,
                    Name = p.Value.Name,
                    IsSelected = p.Key == select.ParamID
                }
            ),
            SelectedParamID = select.ParamID
        };
    }

    private ParameterStates ReduceInterpolateParameter(ParameterStates previous, InterpolateParameterIntent interpolate)
    {
        AnimationManager.Instance.InterpolateParameter(interpolate.Value, interpolate.ParamID);
        return previous;
    }
}
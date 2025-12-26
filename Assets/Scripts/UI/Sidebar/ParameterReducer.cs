using System;
using System.Linq;
using Assets.Scripts.Utility;

public class ParameterReducer : IReducer<ParameterStates>
{
    public ParameterStates Reduce(ParameterStates previous, IIntent intent)
    {
        return intent switch
        {
            InterpolateParameterIntent interpolate => ReduceInterpolateParameter(previous, interpolate),
            SelectParameterIntent select => ReduceSelectParameter(previous, select),
            CreateParameterIntent create => ReduceCreateParameter(previous, create),
            _ => previous
        };
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
            )
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
        if (previous.Parameters[select.ParamID].IsSelected) return previous;

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
        };
    }

    private ParameterStates ReduceInterpolateParameter(ParameterStates previous, InterpolateParameterIntent interpolate)
    {
        AnimationManager.Instance.InterpolateParameter(interpolate.Value, interpolate.ParamID);
        return previous;
    }
}
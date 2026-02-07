using Assets.Scripts.States;

public sealed class ParametersViewModel : ViewModelBase<ParameterTimelineState, ParameterStates>
{
    protected override ParameterStates Project(ParameterTimelineState domain)
    {
        return domain.Parameters;
    }
}


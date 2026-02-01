using Assets.Scripts.States;

public sealed class ParametersViewModel : ViewModelBase<ParameterStates, ParameterStates>
{
    protected override ParameterStates Project(ParameterStates domain)
    {
        return domain;
    }
}


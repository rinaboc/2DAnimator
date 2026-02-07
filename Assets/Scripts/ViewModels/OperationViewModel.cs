using Assets.Scripts.States;

public class OperationViewModel : ViewModelBase<OperationState, OperationState>
{
    protected override OperationState Project(OperationState domain)
    {
        return domain;
    }
}

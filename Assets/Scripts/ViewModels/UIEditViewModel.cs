using Assets.Scripts.States.EditMode;

public sealed class UiEditViewModel : ViewModelBase<MeshUIEditState, UIEditState>
{
    protected override UIEditState Project(MeshUIEditState domain)
    {
        return new UIEditState()
        {
            ID = domain.ID,
            Name = domain.Name
        };
    }
}

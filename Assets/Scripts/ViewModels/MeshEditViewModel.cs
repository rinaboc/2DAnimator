using Assets.Scripts.States.EditMode;

public sealed class MeshEditViewModel : ViewModelBase<MeshUIEditState, MeshEditState>
{
    protected override MeshEditState Project(MeshUIEditState domain)
    {
        return domain.BuildMeshEditState();
    }
}

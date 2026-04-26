using Assets.Scripts.States.EditMode;

public sealed class MeshEditViewModel : ViewModelBase<MeshUIEditState, MeshEditState>
{
    protected override MeshEditState Project(MeshUIEditState domain)
    {
        return new MeshEditState()
        {
            Texture = domain.Texture,
            OriginalTopology = domain.OriginalTopology,
            CurrentTopology = domain.CurrentTopology,
            SelectedVertex = domain.SelectedVertex
        };
    }
}

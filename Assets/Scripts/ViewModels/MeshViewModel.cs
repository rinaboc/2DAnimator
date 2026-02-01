using System.Linq;
using Assets.Scripts.States;

public sealed class MeshViewModel : ViewModelBase<MeshLayerStates, MeshStates>
{
    protected override MeshStates Project(MeshLayerStates domain)
    {
        return new MeshStates()
        {
            Meshes = domain.MeshLayers.ToDictionary(
                p => p.Value.ID,
                p => p.Value.BuildMeshState(domain.MeshLayers.Count)
            ),
            SelectedMeshID = domain.SelectedMeshLayerID
        };
    }
}


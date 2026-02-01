using System.Linq;
using Assets.Scripts.States;

public sealed class LayersViewModel : ViewModelBase<MeshLayerStates, LayerStates>
{
    protected override LayerStates Project(MeshLayerStates domain)
    {
        return new LayerStates()
        {
            Layers = domain.MeshLayers.ToDictionary(
                p => p.Value.ID,
                p => p.Value.BuildLayerState()
            ),
            SelectedLayerID = domain.SelectedMeshLayerID
        };
    }
}

using System;
using System.Linq;
using Assets.Scripts.States;
using Assets.Scripts.Utility.MVI;

public class LayerReducer : IReducer<LayerStates>
{
    public LayerStates Reduce(LayerStates previous, IIntent intent)
    {
        return intent switch
        {
            CreateMeshLayerIntent create => ReduceCreateMeshLayer(previous, create),
            ChangeLayerNameIntent change => ReduceChangeLayerName(previous, change),
            SelectLayerIntent select => ReduceSelectLayer(previous, select),
            DeleteLayerIntent _ => ReduceDeleteLayer(previous),
            _ => previous
        };
    }

    private LayerStates ReduceCreateMeshLayer(LayerStates previous, CreateMeshLayerIntent create)
    {
        var next = previous.Clone();

        next.Layers.Add(create.ID, new LayerState()
        {
            ID = create.ID,
            Name = create.Path,
            IsSelected = false
        });

        return next;
    }

    private LayerStates ReduceDeleteLayer(LayerStates previous)
    {
        var next = previous.Clone();
        next.Layers.Remove(previous.SelectedLayerID);
        next.SelectedLayerID = Guid.Empty;

        return next;
    }

    private LayerStates ReduceSelectLayer(LayerStates previous, SelectLayerIntent select)
    {
        return new()
        {
            Layers = previous.Layers.ToDictionary(
                p => p.Key,
                p => new LayerState(p.Value)
                {
                    IsSelected = p.Key == select.LayerID
                }
            ),
            SelectedLayerID = select.LayerID
        };
    }

    private LayerStates ReduceChangeLayerName(LayerStates previous, ChangeLayerNameIntent change)
    {
        return new()
        {
            Layers = previous.Layers.ToDictionary(
                p => p.Key,
                p => new LayerState(p.Value)
                {
                    Name = p.Key.Equals(change.LayerID) ? change.NewName : p.Value.Name
                }
            ),
            SelectedLayerID = previous.SelectedLayerID
        };
    }
}
using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.States
{
    public class LayerState
    {
        public Guid ID { get; set; }
        public string Name { get; set; }
        public bool IsSelected { get; set; }
        public int DrawOrder { get; set; }
        public bool IsActive { get; set; }

        public LayerState() { }
        public LayerState(LayerState ls)
        {
            ID = ls.ID;
            Name = ls.Name;
            IsSelected = ls.IsSelected;
            DrawOrder = ls.DrawOrder;
            IsActive = ls.IsActive;
        }
    }

    public class LayerStates : IState<LayerStates>
    {
        public Dictionary<Guid, LayerState> Layers { get; set; }
        public Guid SelectedLayerID { get; set; }

        public LayerStates()
        {
            Layers = new();
            SelectedLayerID = Guid.Empty;
        }

        public LayerStates Copy() => new()
        {
            Layers = Layers.ToDictionary(
                p => p.Key,
                p => new LayerState(p.Value)
            ),
            SelectedLayerID = SelectedLayerID
        };
    }
}

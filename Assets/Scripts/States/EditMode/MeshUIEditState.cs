using System;
using Assets.Scripts.Data.MeshInfo;
using UnityEngine;

namespace Assets.Scripts.States.EditMode
{
    public record MeshUIEditState : IState<MeshUIEditState>
    {
        #region UI attributes
        public Guid ID;
        public string Name;
        #endregion

        #region Mesh attributes
        public Texture2D Texture;
        public MeshInfo Topology;
        public MeshInfo TopologyDelta;
        public Vertex SelectedVertex;
        #endregion

        public MeshEditState BuildMeshEditState()
        {
            return new MeshEditState()
            {
                Texture = Texture,
                Topology = Topology,
                TopologyDelta = TopologyDelta,
                SelectedVertex = SelectedVertex
            };
        }

        public UIEditState BuildUIEditState()
        {
            return new UIEditState()
            {
                ID = ID,
                Name = Name
            };
        }

        public MeshUIEditState Copy()
        {
            return new()
            {
                ID = ID,
                Name = Name,
                Texture = Texture,
                Topology = Topology,
                TopologyDelta = TopologyDelta,
                SelectedVertex = SelectedVertex
            };
        }
    }
}

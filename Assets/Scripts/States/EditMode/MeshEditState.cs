using System;
using Assets.Scripts.Data.MeshInfo;
using UnityEngine;

namespace Assets.Scripts.States.EditMode
{
    public record MeshEditState : IState<MeshEditState>
    {
        public Texture2D Texture;
        public MeshInfo Topology;
        public MeshInfo TopologyDelta;
        public Vertex SelectedVertex;

        public MeshEditState Copy()
        {
            return new()
            {
                Texture = Texture,
                Topology = Topology, // TODO: this will copy by reference, remove it
                TopologyDelta = TopologyDelta,
                SelectedVertex = SelectedVertex
            };
        }
    }
}
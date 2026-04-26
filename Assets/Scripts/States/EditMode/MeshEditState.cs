using System;
using Assets.Scripts.Data.MeshInfo;
using UnityEngine;

namespace Assets.Scripts.States.EditMode
{
    public record MeshEditState : IState<MeshEditState>
    {
        public Texture2D Texture;
        public MeshInfo OriginalTopology;
        public MeshInfo CurrentTopology;
        public Vertex SelectedVertex;

        public MeshEditState Copy()
        {
            return new()
            {
                Texture = Texture,
                OriginalTopology = OriginalTopology,
                CurrentTopology = CurrentTopology,
                SelectedVertex = SelectedVertex
            };
        }
    }
}
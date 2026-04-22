using System;
using Assets.Scripts.Data.MeshInfo;
using UnityEngine;

namespace Assets.Scripts.States
{
    public class MeshEditState
    {
        public Guid ID { get; set; }
        public string Name { get; set; }
        public Texture2D Texture { get; set; }
        public MeshInfo Topology { get; set; }
    }
}
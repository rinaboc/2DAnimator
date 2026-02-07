using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.States
{
    public class MeshLayerState
    {
        #region Shared Attributes
        public Guid ID { get; set; }
        public bool IsSelected { get; set; }
        public int DrawOrder { get; set; }
        #endregion

        #region Layer Attributes
        public string Name { get; set; }
        #endregion

        #region Mesh Attributes
        public Texture2D Texture { get; set; }
        public string SourcePath { get; set; }
        public TransformData MeshTransform { get; set; }
        public TransformData AnimationTransform { get; set; }
        public TransformData InterpolatedTransform { get; set; }
        public bool IsInterpolated { get; set; }
        #endregion

        public MeshLayerState()
        {
            ID = Guid.Empty;
            IsSelected = false;
            DrawOrder = 0;

            MeshTransform = new()
            {
                Scale = Vector3.one
            };
            AnimationTransform = new();
            InterpolatedTransform = new();
            IsInterpolated = false;
        }

        public MeshLayerState(MeshLayerState ms)
        {
            ID = ms.ID;
            IsSelected = ms.IsSelected;
            DrawOrder = ms.DrawOrder;
            Texture = ms.Texture;
            SourcePath = ms.SourcePath;
            Name = ms.Name;
            MeshTransform = ms.MeshTransform;
            AnimationTransform = ms.AnimationTransform;
            InterpolatedTransform = ms.InterpolatedTransform;
            IsInterpolated = ms.IsInterpolated;
        }

        public LayerState BuildLayerState()
        {
            return new LayerState()
            {
                ID = ID,
                Name = Name,
                IsSelected = IsSelected,
                DrawOrder = DrawOrder
            };
        }

        public MeshState BuildMeshState(int meshCount)
        {
            return new MeshState()
            {
                ID = ID,
                MeshTransform = MeshTransform,
                AnimationTransform = AnimationTransform,
                InterpolatedTransform = InterpolatedTransform,
                IsInterpolated = IsInterpolated,
                IsSelected = IsSelected,
                DrawOrder = (ushort)(meshCount - 1 - DrawOrder)
            };
        }
    }

    public class MeshLayerStates
    {
        public Dictionary<Guid, MeshLayerState> MeshLayers { get; set; }
        public Guid SelectedMeshLayerID { get; set; }

        public MeshLayerStates()
        {
            MeshLayers = new();
            SelectedMeshLayerID = Guid.Empty;
        }

        public MeshLayerStates Clone() => new()
        {
            MeshLayers = MeshLayers.ToDictionary(
                p => p.Key,
                p => new MeshLayerState(p.Value)
            ),
            SelectedMeshLayerID = SelectedMeshLayerID
        };
    }
}

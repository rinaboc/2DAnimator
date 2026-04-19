using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.States
{
    public class MeshState
    {
        public Guid ID { get; set; }
        public TransformData MeshTransform { get; set; }
        public TransformData AnimationTransform { get; set; }
        public TransformData InterpolatedTransform { get; set; }
        public bool IsInterpolated { get; set; }
        public bool IsSelected { get; set; }
        public ushort DrawOrder { get; set; }
        public bool HasParametersAssigned { get; set; }

        public MeshState()
        {
            ID = Guid.Empty;
            MeshTransform = new()
            {
                Scale = Vector3.one
            };
            AnimationTransform = new();
            InterpolatedTransform = new();
            IsInterpolated = false;
            IsSelected = false;
            DrawOrder = 0;
            HasParametersAssigned = false;
        }
        public MeshState(MeshState ms)
        {
            ID = ms.ID;
            MeshTransform = ms.MeshTransform.Clone();
            AnimationTransform = ms.AnimationTransform.Clone();
            InterpolatedTransform = ms.InterpolatedTransform.Clone();
            IsInterpolated = ms.IsInterpolated;
            IsSelected = ms.IsSelected;
            DrawOrder = ms.DrawOrder;
            HasParametersAssigned = ms.HasParametersAssigned;
        }
    }

    public class MeshStates
    {
        public Dictionary<Guid, MeshState> Meshes { get; set; }
        public Guid SelectedMeshID { get; set; }

        public MeshStates()
        {
            Meshes = new();
            SelectedMeshID = Guid.Empty;
        }

        public MeshStates Clone() => new()
        {
            Meshes = Meshes.ToDictionary(
                p => p.Key,
                p => new MeshState(p.Value)
            ),
            SelectedMeshID = SelectedMeshID
        };

    }

}
using System;
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


        public MeshState(Guid id, TransformData transform, TransformData animationTransform)
        {
            ID = id;
            this.MeshTransform = transform;
            this.AnimationTransform = animationTransform;
            InterpolatedTransform = new();
            IsInterpolated = false;
        }

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
        }
        public MeshState(MeshState ms)
        {
            ID = ms.ID;
            MeshTransform = ms.MeshTransform;
            AnimationTransform = ms.AnimationTransform;
            InterpolatedTransform = ms.InterpolatedTransform;
            IsInterpolated = ms.IsInterpolated;
            IsSelected = ms.IsSelected;
        }
    }
}
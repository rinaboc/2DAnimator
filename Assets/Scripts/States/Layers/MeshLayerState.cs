using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Utility.MVI;
using UnityEngine;

namespace Assets.Scripts.States
{
    public class MeshLayerState
    {
        #region Shared Attributes
        public Guid ID { get; set; }
        public bool IsSelected { get; set; }
        public int DrawOrder { get; set; }
        public bool IsActive { get; set; }
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
        public bool HasParametersAssigned { get; set; }
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
            HasParametersAssigned = false;
            IsActive = true;
        }

        public MeshLayerState(MeshLayerState ms)
        {
            ID = ms.ID;
            IsSelected = ms.IsSelected;
            DrawOrder = ms.DrawOrder;
            Texture = ms.Texture;
            SourcePath = ms.SourcePath;
            Name = ms.Name;
            MeshTransform = ms.MeshTransform.Clone();
            AnimationTransform = ms.AnimationTransform.Clone();
            InterpolatedTransform = ms.InterpolatedTransform.Clone();
            IsInterpolated = ms.IsInterpolated;
            HasParametersAssigned = ms.HasParametersAssigned;
            IsActive = ms.IsActive;
        }

        public MeshLayerState(MeshData meshData, MeshLayerState ms)
        {
            ID = meshData.ID;
            DrawOrder = meshData.drawOrder;
            Texture = meshData.texture.Data;
            SourcePath = meshData.sourcePath;
            Name = meshData.name;
            MeshTransform = meshData.transform.Clone();

            IsSelected = ms.IsSelected;
            AnimationTransform = ms.AnimationTransform.Clone();
            InterpolatedTransform = ms.InterpolatedTransform.Clone();
            IsInterpolated = ms.IsInterpolated;
            HasParametersAssigned = ms.HasParametersAssigned;
            IsActive = ms.IsActive;
        }

        public LayerState BuildLayerState()
        {
            return new LayerState()
            {
                ID = ID,
                Name = Name,
                IsSelected = IsSelected,
                DrawOrder = DrawOrder,
                IsActive = IsActive
            };
        }

        public MeshState BuildMeshState(int meshCount)
        {
            return new MeshState()
            {
                ID = ID,
                MeshTransform = MeshTransform.Clone(),
                AnimationTransform = AnimationTransform.Clone(),
                InterpolatedTransform = InterpolatedTransform.Clone(),
                IsInterpolated = IsInterpolated,
                IsSelected = IsSelected,
                DrawOrder = (ushort)(meshCount - 1 - DrawOrder),
                HasParametersAssigned = HasParametersAssigned,
                IsActive = IsActive
            };
        }
    }

    public class MeshLayerStates : IState<MeshLayerStates>
    {
        public Dictionary<Guid, MeshLayerState> MeshLayers { get; set; }
        public Guid SelectedMeshLayerID { get; set; }

        public MeshLayerStates()
        {
            MeshLayers = new();
            SelectedMeshLayerID = Guid.Empty;
        }

        public MeshLayerStates Copy() => new()
        {
            MeshLayers = MeshLayers.ToDictionary(
                p => p.Key,
                p => new MeshLayerState(p.Value)
            ),
            SelectedMeshLayerID = SelectedMeshLayerID
        };

        public MeshLayerStates(IModelContext context, MeshLayerStates ms) : this()
        {
            foreach (MeshData meshData in context.Meshes.GetAll())
            {
                if (!ms.MeshLayers.TryGetValue(meshData.ID, out var meshLayerState))
                {
                    MeshLayerState newMeshLayerState = new();
                    MeshLayers.Add(meshData.ID, new MeshLayerState(meshData, newMeshLayerState));
                }
                else
                {
                    MeshLayers.Add(meshData.ID, new MeshLayerState(meshData, meshLayerState));
                }
                var meshlayer = MeshLayers[meshData.ID];
                meshlayer.HasParametersAssigned = context.ParamCurves.GetAssignedParamIDsOfMesh(meshData.ID).Count > 0;
                meshlayer.IsSelected = meshData.ID == context.SessionInfo.SelectedMeshID;
            }

            SelectedMeshLayerID = context.SessionInfo.SelectedMeshID;
        }
    }
}

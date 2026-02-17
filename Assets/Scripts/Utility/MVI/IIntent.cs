using System;
using System.Collections.Generic;
using Assets.Scripts.Utility.MVI;
using UnityEngine;

namespace Assets.Scripts.Utility.MVI
{
    public interface IIntent { }
    public interface IIntentUndo : IIntent { }
    public interface IIntentRedo : IIntent { }

    public class IntentHelper
    {
        private static readonly Dictionary<Type, bool> _globalCache = new();
        private static readonly Dictionary<Type, bool> _undoableCache = new();

        public static bool IsGlobalIntent(IIntent intent)
        {
            var type = intent.GetType();
            if (_globalCache.TryGetValue(type, out var isGlobal))
                return isGlobal;

            isGlobal = Attribute.IsDefined(type, typeof(GlobalIntentAttribute));
            _globalCache[type] = isGlobal;
            return isGlobal;
        }

        public static bool IsUndoableIntent(IIntent intent)
        {
            var type = intent.GetType();
            if (_undoableCache.TryGetValue(type, out var isUndoable))
                return isUndoable;

            isUndoable = !Attribute.IsDefined(type, typeof(NonUndoableIntentAttribute));
            _undoableCache[type] = isUndoable;
            return isUndoable;
        }
    }
}

#region Workspace Operations
[NonUndoableIntent] public record SaveProjectIntent(string Path) : IIntent;
[NonUndoableIntent] public record OpenProjectIntent(string Path) : IIntent;
[GlobalIntent, NonUndoableIntent] public record InitializeProjectIntent(SaveData SaveData) : IIntent;
public record UndoIntent() : IIntentUndo;
public record RedoIntent() : IIntentRedo;
#endregion

#region Mesh Transformation
[NonUndoableIntent] public record UpdateTransformIntent(Guid MeshID, TransformData Data, TransformType Type) : IIntent;
public record SaveTransformIntent(Guid MeshID, TransformType Type) : IIntent;
[NonUndoableIntent] public record InterpolateTransformIntent(Guid MeshID, TransformData Delta) : IIntent;
[NonUndoableIntent] public record ResetInterpolationIntent(Guid MeshID) : IIntent;
#endregion

#region Layer Operations
[GlobalIntent] public record CreateMeshLayerIntent(Guid ID, Texture2D Tex, string Path) : IIntent;
[GlobalIntent] public record SelectLayerIntent(Guid LayerID) : IIntent;
public record ChangeLayerNameIntent(Guid LayerID, string NewName) : IIntent;
[GlobalIntent] public record DeleteLayerIntent() : IIntent;
[GlobalIntent] public record MoveLayerUpIntent() : IIntent;
[GlobalIntent] public record MoveLayerDownIntent() : IIntent;
#endregion

#region Parameter Operations
[NonUndoableIntent] public record OpenParameterCreatorIntent() : IIntent;
[NonUndoableIntent] public record OpenParameterEditorIntent() : IIntent;
[NonUndoableIntent] public record CloseParameterSettingsIntent() : IIntent;
public record SelectParameterIntent(Guid ParamID) : IIntent;
public record DeselectParameterIntent() : IIntent;
[GlobalIntent] public record CreateParameterIntent(Guid ParamID, float Min, float Max, float Default, string Name) : IIntent;
public record UpdateParameterIntent(Guid ParamID, float Min, float Max, float Default, string Name) : IIntent;
public record DeleteSelectedParameterIntent() : IIntent;
[GlobalIntent] public record DeletedParameterIntent(Guid ParamID) : IIntent;
public record CreateParamPointsIntent() : IIntent;
[NonUndoableIntent] public record ParameterValueInterpolatedIntent(Guid ParamID, float Value) : IIntent;
#endregion

#region Animation
[NonUndoableIntent] public record InterpolateParameterIntent(Guid ParamID, float Value) : IIntent;
#endregion

#region Timeline Operations
[NonUndoableIntent] public record TimelineOpenIntent() : IIntent;
public record UpdateFramePerSecIntent(int FPS) : IIntent;
public record UpdateMaxFramesIntent(int MaxFrames) : IIntent;
[NonUndoableIntent] public record TimelineSettingsOpenIntent() : IIntent;
public record SelectKeyframeIntent(Guid ID) : IIntent;
public record DeleteKeyframeIntent() : IIntent;
[NonUndoableIntent] public record CurrentFrameChangedIntent(int Frame) : IIntent;
public record TimelineParameterSliderChangedIntent(Guid ParamID, float Value) : IIntent;
#endregion
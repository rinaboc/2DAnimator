using System;
using System.Collections.Generic;
using Assets.Scripts.Utility.MVI;
using UnityEngine;

namespace Assets.Scripts.Utility.MVI
{
    public interface IIntent { }
    public interface IIntentUndo : IIntent { }
    public interface IIntentRedo : IIntent { }
    public interface IIntentDialog : IIntent { }
    public interface IIntentUnstored : IIntent { }

    public class IntentHelper
    {
        private static readonly Dictionary<Type, bool> _globalCache = new();
        private static readonly Dictionary<Type, bool> _undoableCache = new();

        public static bool IsGlobalIntent(IIntent intent) => HasAttribute(intent, typeof(GlobalIntentAttribute), _globalCache);
        public static bool IsUndoableIntent(IIntent intent) => !HasAttribute(intent, typeof(NonUndoableIntentAttribute), _undoableCache);

        private static bool HasAttribute(IIntent intent, Type attribute, Dictionary<Type, bool> cache)
        {
            var type = intent.GetType();
            if (cache.TryGetValue(type, out var isAttribute))
                return isAttribute;

            isAttribute = Attribute.IsDefined(type, attribute);
            cache[type] = isAttribute;
            return isAttribute;
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
[NonUndoableIntent] public record UpdateTransformIntent(Guid MeshID, TransformData Data, TransformType Type) : IIntentUnstored;
[NonUndoableIntent] public record SaveTransformIntent(Guid MeshID, TransformData Data, TransformType Type) : IIntent; // TODO: end drag
[NonUndoableIntent] public record InterpolateTransformIntent(Dictionary<Guid, TransformData> Deltas) : IIntentUnstored;
public record ResetInterpolationIntent(Guid MeshID) : IIntent; // TODO: start drag
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
public record OpenParameterCreatorIntent() : IIntentDialog;
public record OpenParameterEditorIntent() : IIntentDialog;
[NonUndoableIntent] public record CloseParameterSettingsIntent() : IIntentDialog;
public record SelectParameterIntent(Guid ParamID) : IIntent;
[GlobalIntent, NonUndoableIntent] public record CreateParameterIntent(Guid ParamID, float Min, float Max, float Default, string Name) : IIntent;
[NonUndoableIntent] public record UpdateParameterIntent(Guid ParamID, float Min, float Max, float Default, string Name) : IIntent;
public record DeleteSelectedParameterIntent() : IIntent;
[GlobalIntent] public record CreateParamPointsIntent() : IIntent;
[NonUndoableIntent] public record ParameterValueInterpolatedIntent(KeyValuePair<Guid, float>[] ParamValues) : IIntentUnstored;
#endregion

#region Animation
[NonUndoableIntent] public record InterpolateParameterIntent(Guid ParamID, float Value) : IIntentUnstored;
[NonUndoableIntent] public record StartParameterDragIntent(Guid ParamID) : IIntent;
#endregion

#region Timeline Operations
[NonUndoableIntent] public record TimelineOpenIntent() : IIntent;
public record UpdateFramePerSecIntent(int FPS) : IIntent;
public record UpdateMaxFramesIntent(int MaxFrames) : IIntent;
[NonUndoableIntent] public record TimelineSettingsOpenIntent() : IIntent;
public record SelectKeyframeIntent(Guid ID) : IIntent;
public record DeleteKeyframeIntent() : IIntent;
public record StartTimelineSliderDragIntent : IIntent;
[NonUndoableIntent] public record CurrentFrameChangedIntent(int Frame) : IIntentUnstored;
public record StartTimelineParameterDragIntent : IIntent;
[NonUndoableIntent] public record TimelineParameterSliderChangedIntent(Guid ParamID, float Value) : IIntentUnstored;
#endregion
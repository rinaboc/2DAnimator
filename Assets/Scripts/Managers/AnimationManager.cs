using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.States;
using Assets.Scripts.Utility.MVI;
using UnityEngine;

public class AnimationManager : ManagerBase<AnimationManager>
{
    private Dictionary<Guid, float> _currentCurveSliderValues = new();
    public bool GetCurrentCurveSliderValue(Guid id, out float value) => _currentCurveSliderValues.TryGetValue(id, out value);

    [SerializeField] private TimelineWidgetController _timelineWidget;

    [SerializeField] private AppInitializer _appInitializer;
    private IViewModel<ParameterStates> _viewModel;


    void Start()
    {
        if (!_appInitializer.GetViewModel(out _viewModel))
        {
            Debug.LogError("Couldn't fetch viewModel");
        }
    }

    public void AnimateTimeline(int currentFrame)
    {
        var parameters = ParameterRegistry.Instance.GetAll();

        foreach (Parameter parameter in parameters)
        {
            List<KeyFrame> parameterKeys = KeyFrameRegistry.Instance.GetKeyFramesOfParam(parameter.ID);
            if (parameterKeys.Count < 2) continue;

            parameterKeys.Sort((a, b) => a.Frame.CompareTo(b.Frame));

            KeyFrame minFrame = parameterKeys.LastOrDefault(k => k.Frame <= currentFrame);
            KeyFrame maxFrame = parameterKeys.FirstOrDefault(k => k.Frame >= currentFrame);

            // current frame is outside the range of keyframes
            if (minFrame == null) // Before the first keyframe
            {
                minFrame = maxFrame;
            }
            else if (maxFrame == null) // After the last keyframe
            {
                maxFrame = minFrame;
            }

            float interpolatedValue;
            if (minFrame.Frame == maxFrame.Frame)
            {
                interpolatedValue = minFrame.ParamValue;
            }
            else
            {
                float t = (float)(currentFrame - minFrame.Frame) / (maxFrame.Frame - minFrame.Frame);
                interpolatedValue = Mathf.Lerp(minFrame.ParamValue, maxFrame.ParamValue, t);
            }

            InterpolateParameter(interpolatedValue, parameter.ID);
            _viewModel?.Send(new ParameterValueInterpolatedIntent(parameter.ID, interpolatedValue));
        }
    }

    public KeyFrame CreateKeyframe(Guid paramID, float value)
    {
        Debug.Log("Creating new keyframe");
        int CurrentFrame = _timelineWidget.Currentframe;

        KeyFrame keyFrame = new(paramID, value, CurrentFrame);
        return keyFrame;
    }

    public void RemoveKeyFrame(Guid id)
    {
        KeyFrameRegistry.Instance.Remove(id);
    }

    /// <summary>
    /// Interpolate parameter point values assigned to the selected parameter and set the interpolated transformations on the meshes.
    /// </summary>
    public void InterpolateParameter(float value, Guid paramID)
    {
        _currentCurveSliderValues[paramID] = value;
        var accumTransforms = new Dictionary<Guid, TransformData>();
        foreach (var curveValue in _currentCurveSliderValues)
        {
            CollectParameterDeltas(curveValue.Value, curveValue.Key, accumTransforms);
        }

        ApplyAccumulatedTransforms(accumTransforms);
    }

    private void CollectParameterDeltas(float value, Guid paramID, Dictionary<Guid, TransformData> accumTransforms)
    {
        ParameterRegistry parameterRegistry = ParameterRegistry.Instance;
        ParamPointRegistry paramPointRegistry = ParamPointRegistry.Instance;
        if (!parameterRegistry.TryGet(paramID, out Parameter parameter)) return;
        List<ParamCurve> paramCurves = ParamCurveRegistry.Instance.GetEntries(parameter.ParamCurves);

        foreach (ParamCurve paramCurve in paramCurves)
        {
            List<ParamPoint> paramPoints = paramPointRegistry.GetEntries(paramCurve.ParamPoints);
            if (paramPoints.Count == 0) continue;

            paramPoints.Sort((p1, p2) => p1.ParamValue.CompareTo(p2.ParamValue));
            int maxPIdx = paramPoints.FindIndex(p => p.ParamValue >= value);
            int minPIdx;

            if (maxPIdx == -1) // slider value greater than all param point values
            {
                minPIdx = maxPIdx = paramPoints.Count - 1;
            }
            else if (maxPIdx == 0)
            {
                minPIdx = 0;
            }
            else
            {
                minPIdx = maxPIdx - 1;
            }

            ParamPoint maxPoint = paramPoints[maxPIdx];
            ParamPoint minPoint = paramPoints[minPIdx];

            float t = 0f;
            if (maxPIdx != minPIdx)
            {
                float range = maxPoint.ParamValue - minPoint.ParamValue;
                if (range > 1e-3f) { t = (value - minPoint.ParamValue) / range; }
            }

            Vector3 interpPos = Vector3.Lerp(minPoint.transform.Position, maxPoint.transform.Position, t);
            Vector3 interpScale = Vector3.Lerp(minPoint.transform.Scale, maxPoint.transform.Scale, t);
            Quaternion interpRotation = Quaternion.Lerp(minPoint.transform.Rotation, maxPoint.transform.Rotation, t);

            // accumulate per mesh
            if (!accumTransforms.TryGetValue(paramCurve.MeshID, out TransformData cur)) cur = new TransformData();
            cur.Position += interpPos;
            cur.Scale += interpScale;
            cur.Rotation *= interpRotation;
            accumTransforms[paramCurve.MeshID] = cur;
        }
    }

    private void ApplyAccumulatedTransforms(Dictionary<Guid, TransformData> accum)
    {
        foreach (var kv in accum)
        {
            var meshId = kv.Key;
            var delta = kv.Value;

            MeshManager.Instance.GetMeshObject(meshId, out MeshController artMesh);
            MeshRegistry.Instance.TryGet(meshId, out MeshData meshData);
            if (artMesh == null || meshData == null)
            {
                Debug.LogError("artmesh or meshdata null in apply accumulated transforms");
                continue;
            }

            MeshManager.Instance.DispatchToMeshViewModel(new InterpolateTransformIntent(meshId, delta));
            // artMesh.MoveArtMesh(meshData.transform.Position + delta.Position);
            // artMesh.ScaleArtMesh(meshData.transform.Scale + delta.Scale);
            // artMesh.RotateArtMesh(meshData.transform.Rotation * delta.Rotation);
        }
    }

    public override void LoadState(SaveData saveData)
    {
        GeneralSettings.Instance.MaxFrames = saveData.AnimationSetting.maxFrames;
        GeneralSettings.Instance.FramePerSec = saveData.AnimationSetting.framePerSec;

        KeyFrameRegistry keyFrameRegistry = KeyFrameRegistry.Instance;
        keyFrameRegistry.Clear();
        if (saveData.KeyFrames == null) return;
        foreach (var item in saveData.KeyFrames)
        {
            keyFrameRegistry.Register(item);
        }
    }
}

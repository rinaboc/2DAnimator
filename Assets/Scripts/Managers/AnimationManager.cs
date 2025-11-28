using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AnimationManager : ManagerBase<AnimationManager>
{
    private Dictionary<Guid, float> _currentCurveSliderValues = new();
    private struct AccumulatedTransform
    {
        public Vector3 Pos;
        public Vector3 Scale;
        public Quaternion Rot;

        public void Add(Vector3 pos, Vector3 scale, Quaternion rot)
        {
            Pos += pos;
            Scale += scale;
            Rot *= rot;
        }
        public static AccumulatedTransform Identity => new() { Pos = Vector3.zero, Scale = Vector3.zero, Rot = Quaternion.identity };
    }

    public bool GetCurrentCurveSliderValue(Guid id, out float value) => _currentCurveSliderValues.TryGetValue(id, out value);

    [SerializeField] private TimelineWidgetController _timelineWidget;

    void Start()
    {
        UIEvents.TimelineChangeEvent += AnimateTimeline;
    }

    void OnDisable()
    {
        UIEvents.TimelineChangeEvent -= AnimateTimeline;
    }

    private void AnimateTimeline(int currentFrame)
    {
        List<Parameter> parameters = ParameterRegistry.Instance.GetAll().ToList();

        foreach (Parameter parameter in parameters)
        {
            List<KeyFrame> parameterKeys = KeyFrameRegistry.Instance.GetKeyFramesOfParam(parameter.ID);
            if (parameterKeys.Count < 2) continue;

            KeyFrame minFrame = null, maxFrame = null;

            foreach (KeyFrame key in parameterKeys)
            {
                if (currentFrame == key.Frame)
                {
                    minFrame = maxFrame = key;
                    break;
                }

                if (currentFrame > key.Frame) { minFrame = key; continue; }
                if (currentFrame < key.Frame) { maxFrame = key; break; }
            }

            if (minFrame != null && maxFrame == null) maxFrame = minFrame;
            else if (maxFrame != null && minFrame == null) minFrame = maxFrame;

            float delta = minFrame == maxFrame ? 0f : (minFrame.ParamValue - maxFrame.ParamValue) / (minFrame.Frame - maxFrame.Frame);
            float t = (currentFrame - minFrame.Frame) * delta + minFrame.ParamValue;

            InterpolateParameter(t, parameter.ID);
            UIEvents.RaiseParamInterpolated(parameter.ID, t);
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
        var accumTransforms = new Dictionary<Guid, AccumulatedTransform>();
        foreach (var curveValue in _currentCurveSliderValues)
        {
            CollectParameterDeltas(curveValue.Value, curveValue.Key, accumTransforms);
        }

        ApplyAccumulatedTransforms(accumTransforms);
    }

    private void CollectParameterDeltas(float value, Guid paramID, Dictionary<Guid, AccumulatedTransform> accumTransforms)
    {
        ParameterRegistry parameterRegistry = ParameterRegistry.Instance;
        ParamPointRegistry paramPointRegistry = ParamPointRegistry.Instance;
        if (!parameterRegistry.TryGet(paramID, out Parameter parameter)) return;
        List<ParamCurve> paramCurves = ParamCurveRegistry.Instance.GetEntries(parameter.ParamCurves);

        foreach (ParamCurve paramCurve in paramCurves)
        {
            List<ParamPoint> paramPoints = paramPointRegistry.GetEntries(paramCurve.ParamPoints);
            List<ParamPoint> orderedPoints = paramPoints.OrderBy(point => point.ParamValue).ToList();
            int minP = -1;
            int maxP = -1;
            for (int i = 0; i < orderedPoints.Count; i++)
            {
                if (value > orderedPoints[i].ParamValue) continue;

                maxP = i;
                minP = i;
                if (i <= 0) break;
                minP = i - 1;
                break;
            }
            if (maxP == -1) continue; // no parameter point found on curve

            ParamPoint maxPoint = orderedPoints[maxP];
            ParamPoint minPoint = orderedPoints[minP];

            float t = maxP != minP ? (value - minPoint.ParamValue) / (maxPoint.ParamValue - minPoint.ParamValue) : 0f;

            Vector3 interpPos = Vector3.Lerp(minPoint.transform.Position, maxPoint.transform.Position, t);
            Vector3 interpScale = Vector3.Lerp(minPoint.transform.Scale, maxPoint.transform.Scale, t);
            Quaternion interpRotation = Quaternion.Lerp(minPoint.transform.Rotation, maxPoint.transform.Rotation, t);

            // accumulate per mesh
            if (!accumTransforms.TryGetValue(paramCurve.MeshID, out AccumulatedTransform cur)) cur = AccumulatedTransform.Identity;
            cur.Add(interpPos, interpScale, interpRotation);
            accumTransforms[paramCurve.MeshID] = cur;
        }
    }

    private void ApplyAccumulatedTransforms(Dictionary<Guid, AccumulatedTransform> accum)
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

            artMesh.MoveArtMesh(meshData.transform.Position + delta.Pos);
            artMesh.ScaleArtMesh(meshData.transform.Scale + delta.Scale);
            artMesh.RotateArtMesh(meshData.transform.Rotation * delta.Rot);
        }
    }

    public override void LoadState(SaveData saveData)
    {
        KeyFrameRegistry keyFrameRegistry = KeyFrameRegistry.Instance;
        keyFrameRegistry.Clear();
        foreach (var item in saveData.KeyFrames)
        {
            keyFrameRegistry.Register(item);
        }
    }
}

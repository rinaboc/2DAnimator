using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Assets.Scripts.States;
using Assets.Scripts.Utility.MVI;
using UnityEngine;

public class AnimationManager : ManagerBase<AnimationManager>
{
    private Dictionary<Guid, float> _currentCurveSliderValues = new();
    [SerializeField] private AppInitializer _appInitializer;
    private IViewModel<ParameterTimelineState, ParameterStates> _viewModel;

    private SynchronizationContext _unityContext;

    void Start()
    {
        _unityContext = SynchronizationContext.Current;
        if (!_appInitializer.GetViewModel(out _viewModel))
        {
            Debug.LogError("Couldn't fetch viewModel");
        }
    }

    public async Task AnimateTimeline(int currentFrame, IModelContext context)
    {
        var parameters = context.Parameters.GetAll();

        var tasks = new List<Task<KeyValuePair<Guid, float>>>();

        foreach (Parameter parameter in parameters)
        {
            tasks.Add(Task.Run(() =>
            {
                List<KeyFrame> parameterKeys = context.KeyFrames.GetKeyFramesOfParam(parameter.ID);
                if (parameterKeys.Count < 2 && parameterKeys.Count > 0)
                    return new KeyValuePair<Guid, float>(parameter.ID, parameterKeys[0].ParamValue);
                else if (parameterKeys.Count == 0) return new KeyValuePair<Guid, float>(parameter.ID, parameter.DefaultValue);

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

                InterpolateParameter(interpolatedValue, parameter.ID, context);
                return new KeyValuePair<Guid, float>(parameter.ID, interpolatedValue);
            }));
        }

        var results = await Task.WhenAll(tasks);
        await Task.CompletedTask;
        _unityContext?.Post(_ =>
        {
            foreach (KeyValuePair<Guid, float> result in results)
                _viewModel?.Send(new ParameterValueInterpolatedIntent(result.Key, result.Value));
        }, null);
    }

    /// <summary>
    /// Interpolate parameter point values assigned to the selected parameter and set the interpolated transformations on the meshes.
    /// </summary>
    public async void InterpolateParameter(float value, Guid paramID, IModelContext context)
    {
        var tasks = new List<Task>();
        _currentCurveSliderValues[paramID] = value;
        var accumTransforms = new Dictionary<Guid, TransformData>();
        foreach (var curveValue in _currentCurveSliderValues.ToList())
        {
            tasks.Add(Task.Run(() =>
            CollectParameterDeltas(curveValue.Value, curveValue.Key, accumTransforms, context)
            ));
        }

        await Task.WhenAll(tasks);
        _unityContext?.Post(_ =>
        {
            ApplyAccumulatedTransforms(accumTransforms, context);
        }, null);
    }

    public void CollectParameterDeltas(float value, Guid paramID, Dictionary<Guid, TransformData> accumTransforms, IModelContext context)
    {
        if (!context.Parameters.TryGet(paramID, out Parameter parameter)) return;
        List<ParamCurve> paramCurves = context.ParamCurves.GetEntries(parameter.ParamCurves);

        foreach (ParamCurve paramCurve in paramCurves)
        {
            List<ParamPoint> paramPoints = context.ParamPoints.GetEntries(paramCurve.ParamPoints);
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
            _unityContext?.Post(_ =>
            {
                if (!accumTransforms.TryGetValue(paramCurve.MeshID, out TransformData cur)) cur = new TransformData();
                cur.Position += interpPos;
                cur.Scale += interpScale;
                cur.Rotation *= interpRotation;
                accumTransforms[paramCurve.MeshID] = cur;
            }, null);
        }
    }

    private void ApplyAccumulatedTransforms(Dictionary<Guid, TransformData> accum, IModelContext context)
    {
        foreach (var kv in accum.ToList())
        {
            var meshId = kv.Key;
            var delta = kv.Value;

            context.Meshes.TryGet(meshId, out MeshData meshData);
            if (meshData == null)
            {
                Debug.LogError("meshdata null in apply accumulated transforms");
                continue;
            }

            MeshManager.Instance.DispatchToMeshViewModel(new InterpolateTransformIntent(meshId, delta));
        }
    }
}

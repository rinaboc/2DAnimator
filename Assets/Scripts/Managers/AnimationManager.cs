using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AnimationManager : ManagerBase<AnimationManager>
{
    private Dictionary<Guid, float> _currentCurveSliderValues = new();
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
                    Debug.Log("matching frame");
                    minFrame = maxFrame = key;
                    break;
                }

                if (currentFrame > key.Frame)
                {
                    minFrame = key;
                    Debug.Log($"min frame: {minFrame}");
                    continue;
                }

                if (currentFrame < key.Frame)
                {
                    maxFrame = key;
                    Debug.Log($"max frame: {maxFrame}");
                    break;
                }
            }

            if (minFrame != null && maxFrame == null) maxFrame = minFrame;
            else if (maxFrame != null && minFrame == null) minFrame = maxFrame;

            float delta = minFrame == maxFrame ? 0f : (minFrame.ParamValue - maxFrame.ParamValue) / (minFrame.Frame - maxFrame.Frame);
            // if (minFrame.ParamValue > maxFrame.ParamValue) delta *= -1f;
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
        ParameterRegistry parameterRegistry = ParameterRegistry.Instance;
        ParamPointRegistry paramPointRegistry = ParamPointRegistry.Instance;
        parameterRegistry.TryGet(paramID, out Parameter parameter);
        List<ParamCurve> paramCurves = ParamCurveRegistry.Instance.GetEntries(parameter.ParamCurves);

        if (paramCurves.Count > 0)
        {
            _currentCurveSliderValues[paramID] = value;
        }

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

            if (maxP == -1) break; // no parameterpoint found on curve

            ParamPoint maxPoint = orderedPoints[maxP];

            MeshManager.Instance.GetMeshObject(paramCurve.MeshID, out MeshController artMesh);
            MeshRegistry.Instance.TryGet(paramCurve.MeshID, out MeshData meshData);

            if (artMesh == null || meshData == null)
            {
                Debug.LogError("artmesh or meshdata null in interpolate parameter");
                continue;
            }

            ParamPoint minPoint = orderedPoints[minP];

            float t = maxP != minP ? // not left end of the slider
                (value - minPoint.ParamValue) / (maxPoint.ParamValue - minPoint.ParamValue) : 0f;

            Vector3 interpPos = Vector3.Lerp(minPoint.transform.Position, maxPoint.transform.Position, t);
            Vector3 interpScale = Vector3.Lerp(minPoint.transform.Scale, maxPoint.transform.Scale, t);
            Quaternion interpRotation = Quaternion.Lerp(minPoint.transform.Rotation, maxPoint.transform.Rotation, t);

            artMesh.MoveArtMesh(meshData.transform.Position + interpPos);
            artMesh.ScaleArtMesh(meshData.transform.Scale + interpScale);
            artMesh.RotateArtMesh(meshData.transform.Rotation * interpRotation);
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

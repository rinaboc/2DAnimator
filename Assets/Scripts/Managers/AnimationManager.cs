using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class AnimationManager : MonoBehaviour
{
    public static UnityEvent<int> TimelineChangeEvent = new();
    public static AnimationManager instance;
    private Dictionary<Guid, float> _ParamCurValues = new();
    public bool GetParamCurValue(Guid id, out float value) => _ParamCurValues.TryGetValue(id, out value);

    [SerializeField] private TimelineWidget _timelineWidget;

    public static UnityEvent<Guid> SelectKeyframeEvent = new();
    public static UnityEvent DeleteSelectedKeyframeEvent = new();
    public static UnityEvent<Guid, float> ParamInterpolatedEvent = new();

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(this);
        }
    }

    void Start()
    {
        TimelineChangeEvent.AddListener(AnimateTimeline);
    }

    private void AnimateTimeline(int currentFrame)
    {
        List<Parameter> parameters = ParameterRegistry.Instance.GetAllParameters;

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
            ParamInterpolatedEvent.Invoke(parameter.ID, t);
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
        KeyFrameRegistry.Instance.RemoveKeyframe(id);
    }

    /// <summary>
    /// Interpolate parameter point values assigned to the selected parameter and set the interpolated transformations on the meshes.
    /// </summary>
    public void InterpolateParameter(float value, Guid paramID)
    {
        ParameterRegistry parameterRegistry = ParameterRegistry.Instance;
        Parameter parameter = parameterRegistry.GetParameter(paramID);
        List<ParamCurve> paramCurves = parameterRegistry.GetParamCurve(parameter.ParamCurves);

        if (paramCurves.Count > 0)
        {
            _ParamCurValues[paramID] = value;
        }

        foreach (ParamCurve paramCurve in paramCurves)
        {
            List<ParamPoint> paramPoints = parameterRegistry.GetParamPoint(paramCurve.ParamPoints);
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

            GameObject artMeshObject = MeshRegistry.Instance.GetArtMesh(paramCurve.MeshID);
            MeshData meshData = MeshRegistry.Instance.GetMeshData(paramCurve.MeshID);
            ArtMesh artMesh = artMeshObject.GetComponent<ArtMesh>();

            ParamPoint minPoint = orderedPoints[minP];

            float t = maxP != minP ? // not left end of the slider
                (value - minPoint.ParamValue) / (maxPoint.ParamValue - minPoint.ParamValue) : 0f;

            Vector3 interpPos = Vector3.Lerp(minPoint.Position, maxPoint.Position, t);
            Vector3 interpScale = Vector3.Lerp(minPoint.Scale, maxPoint.Scale, t);
            Quaternion interpRotation = Quaternion.Lerp(minPoint.Rotation, maxPoint.Rotation, t);

            artMesh.MoveArtMesh(meshData.Position + interpPos);
            artMesh.ScaleArtMesh(meshData.Scale + interpScale);
            artMesh.RotateArtMesh(meshData.Rotation * interpRotation);
        }
    }
}

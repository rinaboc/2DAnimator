using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AnimationManager : MonoBehaviour
{
    public static AnimationManager instance;

    [SerializeField] private TimelineWidget _timelineWidget;
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

    public void CreateKeyframe(Guid paramID, float value)
    {
        Debug.Log("Creating new keyframe");
        int CurrentFrame = _timelineWidget.Currentframe;

        KeyFrame keyFrame = new(paramID, value, CurrentFrame);
        Debug.Log(keyFrame);
    }

    /// <summary>
    /// Interpolate parameter point values assigned to the selected parameter and set the interpolated transformations on the meshes.
    /// </summary>
    public void InterpolateParameter(float value, Guid paramID)
    {
        ParameterRegistry parameterRegistry = ParameterRegistry.Instance;
        Parameter parameter = parameterRegistry.GetParameter(paramID);
        List<ParamCurve> paramCurves = parameterRegistry.GetParamCurve(parameter.ParamCurves);

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

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AnimationManager : MonoBehaviour
{
    public static AnimationManager instance;
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

    public void InterpolateParameter(float value, ushort paramID)
    {
        ParameterRegistry parameterRegistry = ParameterRegistry.instance;
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
                if (i <= 0) break;

                minP = i - 1;
                break;
            }

            if (maxP == -1) break; // no parameterpoint found on curve

            ParamPoint maxPoint = orderedPoints[maxP];

            GameObject artMeshObject = MeshRegistry.instance.GetArtMesh(paramCurve.MeshID);
            MeshData meshData = MeshRegistry.instance.GetMeshData(paramCurve.MeshID);

            if (minP == -1) // slider is at left corner
            {
                artMeshObject.transform.localPosition = maxPoint.Position;
                break;
            }

            ParamPoint minPoint = orderedPoints[minP];

            float t = (value - minPoint.ParamValue) / (maxPoint.ParamValue - minPoint.ParamValue);

            Vector3 interpPos = (1f - t) * minPoint.Position + t * maxPoint.Position;
            artMeshObject.transform.localPosition = interpPos;

            Vector3 interpScale = (1f - t) * minPoint.Scale + t * maxPoint.Scale;
            artMeshObject.GetComponent<ArtMesh>().ScaleArtMesh(Vector3.one, interpScale);

            Quaternion interpRotation = Quaternion.Lerp(minPoint.Rotation, maxPoint.Rotation, t);
            artMeshObject.GetComponent<ArtMesh>().RotateArtMesh(Quaternion.identity, interpRotation);
        }
    }
}

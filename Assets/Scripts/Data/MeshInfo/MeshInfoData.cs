using UnityEngine;

namespace Assets.Scripts.Data.MeshInfo
{
    [System.Serializable]
    public class VertexDTO
    {
        [SerializeField] private float[] position = new float[2];
        public Vector2 Position
        {
            get => new(position[0], position[1]);
            set { position[0] = value.x; position[1] = value.y; }
        }
        public int incidentEdgeIndex;
    }

    [System.Serializable]
    public class HalfEdgeDTO
    {
        public int originIndex;
        public int nextIndex;
        public int twinIndex;
        public int faceIndex;
        public bool isConstrained;
    }

    [System.Serializable]
    public class FaceDTO
    {
        public int edgeIndex;
    }

    [System.Serializable]
    public class MeshSaveData
    {
        public VertexDTO[] vertices;
        public HalfEdgeDTO[] edges;
        public FaceDTO[] faces;
    }
}
using System;
using System.Collections.Generic;
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
        [SerializeField] private float[] uv = new float[2];
        public Vector2 UV
        {
            get => new(uv[0], uv[1]);
            set { uv[0] = value.x; uv[1] = value.y; }
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
    public class MeshInfoData
    {
        public VertexDTO[] vertices;
        public HalfEdgeDTO[] edges;
        public FaceDTO[] faces;

        internal MeshInfo ToMeshInfo()
        {
            var meshInfo = new MeshInfo
            {
                Vertices = new List<Vertex>(vertices.Length),
                HalfEdges = new List<HalfEdge>(edges.Length),
                Faces = new List<Face>(faces.Length),
            };

            for (int i = 0; i < vertices.Length; i++)
                meshInfo.Vertices.Add(new Vertex { Position = vertices[i].Position, UV = vertices[i].UV });

            for (int i = 0; i < edges.Length; i++)
                meshInfo.HalfEdges.Add(new HalfEdge { IsConstrained = edges[i].isConstrained });

            for (int i = 0; i < faces.Length; i++)
                meshInfo.Faces.Add(new Face());

            for (int i = 0; i < edges.Length; i++)
            {
                var dto = edges[i];
                var e = meshInfo.HalfEdges[i];

                e.Origin = meshInfo.Vertices[dto.originIndex];
                e.Next = meshInfo.HalfEdges[dto.nextIndex];
                e.Twin = (dto.twinIndex != -1) ? meshInfo.HalfEdges[dto.twinIndex] : null;
                e.Face = meshInfo.Faces[dto.faceIndex];
            }

            for (int i = 0; i < vertices.Length; i++)
            {
                var v = meshInfo.Vertices[i];
                v.IncidentEdge = meshInfo.HalfEdges[vertices[i].incidentEdgeIndex];
            }

            for (int i = 0; i < faces.Length; i++)
                meshInfo.Faces[i].Edge = meshInfo.HalfEdges[faces[i].edgeIndex];


            return meshInfo;
        }
    }
}
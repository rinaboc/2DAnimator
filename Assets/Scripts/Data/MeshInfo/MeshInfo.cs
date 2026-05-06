using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Data.MeshInfo
{
    public class Vertex
    {
        public Vector2 Position;
        public Vector2 UV;
        public HalfEdge IncidentEdge;
    }

    public class HalfEdge
    {
        public Vertex Origin;
        public HalfEdge Next;
        public HalfEdge Twin;
        public Face Face;
        public bool IsConstrained;
    }

    public class Face
    {
        public HalfEdge Edge;
    }

    public class MeshInfo
    {
        public List<HalfEdge> HalfEdges = new();
        public List<Vertex> Vertices = new();
        public List<Face> Faces = new();
        public MeshInfo Clone()
        {
            var newMesh = new MeshInfo();
            var vMap = CreateMap(Vertices, newMesh.Vertices, v => new Vertex { Position = v.Position, UV = v.UV });
            var eMap = CreateMap(HalfEdges, newMesh.HalfEdges, e => new HalfEdge { IsConstrained = e.IsConstrained });
            var fMap = CreateMap(Faces, newMesh.Faces, f => new Face());

            for (int i = 0; i < Vertices.Count; i++)
            {
                newMesh.Vertices[i].IncidentEdge = eMap[Vertices[i].IncidentEdge];
            }

            for (int i = 0; i < HalfEdges.Count; i++)
            {
                var oldE = HalfEdges[i];
                var newE = newMesh.HalfEdges[i];

                newE.Origin = vMap[oldE.Origin];
                newE.Next = eMap[oldE.Next];
                newE.Face = fMap[oldE.Face];

                newE.Twin = oldE.Twin != null ? eMap[oldE.Twin] : null;
            }

            for (int i = 0; i < Faces.Count; i++)
            {
                newMesh.Faces[i].Edge = eMap[Faces[i].Edge];
            }

            return newMesh;
        }

        private static Dictionary<T, T> CreateMap<T>(List<T> source, List<T> destination, System.Func<T, T> creator)
        {
            var map = new Dictionary<T, T>();
            foreach (var item in source)
            {
                var newItem = creator(item);
                map[item] = newItem;
                destination.Add(newItem);
            }
            return map;
        }

        public MeshInfoData ToData()
        {
            var vToIndex = new Dictionary<Vertex, int>();
            for (int i = 0; i < Vertices.Count; i++) vToIndex[Vertices[i]] = i;

            var eToIndex = new Dictionary<HalfEdge, int>();
            for (int i = 0; i < HalfEdges.Count; i++) eToIndex[HalfEdges[i]] = i;

            var fToIndex = new Dictionary<Face, int>();
            for (int i = 0; i < Faces.Count; i++) fToIndex[Faces[i]] = i;

            MeshInfoData data = new()
            {
                edges = new HalfEdgeDTO[HalfEdges.Count],
                vertices = new VertexDTO[Vertices.Count],
                faces = new FaceDTO[Faces.Count]
            };

            for (int i = 0; i < HalfEdges.Count; i++)
            {
                var e = HalfEdges[i];
                data.edges[i] = new HalfEdgeDTO
                {
                    originIndex = vToIndex[e.Origin],
                    nextIndex = eToIndex[e.Next],
                    twinIndex = (e.Twin != null) ? eToIndex[e.Twin] : -1,
                    faceIndex = fToIndex[e.Face],
                    isConstrained = e.IsConstrained
                };
            }

            for (int i = 0; i < Vertices.Count; i++)
            {
                var v = Vertices[i];
                data.vertices[i] = new VertexDTO
                {
                    Position = v.Position,
                    UV = v.UV,
                    incidentEdgeIndex = eToIndex[v.IncidentEdge]
                };
            }

            for (int i = 0; i < Faces.Count; i++)
            {
                var f = Faces[i];
                data.faces[i] = new FaceDTO
                {
                    edgeIndex = eToIndex[f.Edge]
                };
            }

            return data;
        }
    }
}
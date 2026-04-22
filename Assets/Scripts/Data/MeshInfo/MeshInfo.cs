using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Data.MeshInfo
{
    public class Vertex
    {
        public Vector2 Position;
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

    public record MeshInfo
    {
        public List<HalfEdge> HalfEdges = new();
        public List<Vertex> Vertices = new();
        public List<Face> Faces = new();
    }
}
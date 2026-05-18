using System.Collections.Generic;
using Assets.Scripts.Data.MeshInfo;
using Assets.Scripts.Utility.Mesh;
using UnityEngine;

public class TopologyBuilder
{
    public static void Build(Vector3[] vertices, Vector2[] uv, int[] triangles, out MeshInfo meshInfo)
    {
        meshInfo = new();

        Vertex[] vNodes = new Vertex[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            vNodes[i] = new Vertex()
            {
                Position = new Vector2(vertices[i].x, vertices[i].y),
                UV = (uv != null && i < uv.Length) ? uv[i] : Vector2.zero
            };
        }

        var edgeTracker = new Dictionary<(int, int), HalfEdge>();

        for (int i = 0; i < triangles.Length; i += 3)
        {
            Face face = new();
            meshInfo.Faces.Add(face);

            int i0 = triangles[i];
            int i1 = triangles[i + 1];
            int i2 = triangles[i + 2];

            HalfEdge e1 = new() { Origin = vNodes[i0], Face = face };
            HalfEdge e2 = new() { Origin = vNodes[i1], Face = face };
            HalfEdge e3 = new() { Origin = vNodes[i2], Face = face };

            e1.Next = e2; e2.Next = e3; e3.Next = e1;
            face.Edge = e1;

            vNodes[i0].IncidentEdge = e1;
            vNodes[i1].IncidentEdge = e2;
            vNodes[i2].IncidentEdge = e3;

            ConnectTwin(i0, i1, e1, edgeTracker);
            ConnectTwin(i1, i2, e2, edgeTracker);
            ConnectTwin(i2, i0, e3, edgeTracker);

            meshInfo.HalfEdges.AddRange(new HalfEdge[] { e1, e2, e3 });
        }

        meshInfo.Vertices.AddRange(vNodes);
        foreach (var item in edgeTracker.Values)
        {
            item.IsConstrained = true;
        }
    }

    private static void ConnectTwin(int a, int b, HalfEdge current, Dictionary<(int, int), HalfEdge> tracker)
    {
        var key = a < b ? (a, b) : (b, a);

        if (tracker.ContainsKey(key))
        {
            HalfEdge other = tracker[key];
            current.Twin = other;
            other.Twin = current;
            tracker.Remove(key);
        }
        else
        {
            tracker[key] = current;
        }
    }

    public static MeshInfo InsertVertex(MeshInfo meshInfo, Vector2 point)
    {
        var cdt = new CDT(meshInfo);
        return cdt.AddVertex(point);
    }

    public static MeshInfo RemoveVertex(MeshInfo meshInfo, Vertex v)
    {
        var cdt = new CDT(meshInfo);
        return cdt.RemoveVertex(v);
    }

    public static string SanityCheck(MeshInfo meshInfo)
    {
        foreach (var edge in meshInfo.HalfEdges)
        {
            if (edge.Next == null || edge.Next == edge) return "No next or self loop";
            if (edge.Next.Next == edge || edge.Next.Next.Next != edge) return "Not a closed triangle";
            if (edge.Twin != null && edge.Twin.Twin != edge) return "twins aren't referencing each other";
            if (edge.Twin != null && edge.Origin != edge.Twin.Next.Origin) return "In and out edges aren't from same vertex";
            if (edge.Twin != null && edge.Twin.Origin != edge.Next.Origin) return "In and out edges aren't from same vertex";
        }

        return "";
    }
}
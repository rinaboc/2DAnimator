using Assets.Scripts.Data.MeshInfo;
using UnityEngine;

namespace Assets.Scripts.Utility.Mesh
{
    public class CDT : ITriangulator
    {
        private readonly MeshInfo _mesh;

        public CDT(MeshInfo mesh)
        {
            _mesh = mesh;
        }

        public MeshInfo AddVertex(Vector2 point)
        {
            Face f0 = FindFaceContaining(point);
            if (f0 == null) return null;

            Vertex vNew = new() { Position = point, UV = Vector2.zero };
            _mesh.Vertices.Add(vNew);

            HalfEdge e0 = f0.Edge;
            HalfEdge e1 = e0.Next;
            HalfEdge e2 = e1.Next;

            Vertex v0 = e0.Origin;
            Vertex v1 = e1.Origin;
            Vertex v2 = e2.Origin;

            Face f1 = new();
            Face f2 = new();
            _mesh.Faces.Add(f1);
            _mesh.Faces.Add(f2);

            HalfEdge e_v1_vNew = new();
            HalfEdge e_vNew_v0 = new();

            HalfEdge e_v2_vNew = new();
            HalfEdge e_vNew_v1 = new();

            HalfEdge e_v0_vNew = new();
            HalfEdge e_vNew_v2 = new();

            _mesh.HalfEdges.Add(e_v1_vNew); _mesh.HalfEdges.Add(e_vNew_v0);
            _mesh.HalfEdges.Add(e_v2_vNew); _mesh.HalfEdges.Add(e_vNew_v1);
            _mesh.HalfEdges.Add(e_v0_vNew); _mesh.HalfEdges.Add(e_vNew_v2);

            e_v1_vNew.Origin = v1;
            e_vNew_v0.Origin = vNew;

            e_v2_vNew.Origin = v2;
            e_vNew_v1.Origin = vNew;

            e_v0_vNew.Origin = v0;
            e_vNew_v2.Origin = vNew;

            e_v1_vNew.Twin = e_vNew_v1; e_vNew_v1.Twin = e_v1_vNew;
            e_v2_vNew.Twin = e_vNew_v2; e_vNew_v2.Twin = e_v2_vNew;
            e_v0_vNew.Twin = e_vNew_v0; e_vNew_v0.Twin = e_v0_vNew;

            e0.Face = f0; e_v1_vNew.Face = f0; e_vNew_v0.Face = f0;
            f0.Edge = e0;

            e1.Face = f1; e_v2_vNew.Face = f1; e_vNew_v1.Face = f1;
            f1.Edge = e1;

            e2.Face = f2; e_v0_vNew.Face = f2; e_vNew_v2.Face = f2;
            f2.Edge = e2;

            e0.Next = e_v1_vNew;
            e_v1_vNew.Next = e_vNew_v0;
            e_vNew_v0.Next = e0;

            e1.Next = e_v2_vNew;
            e_v2_vNew.Next = e_vNew_v1;
            e_vNew_v1.Next = e1;

            e2.Next = e_v0_vNew;
            e_v0_vNew.Next = e_vNew_v2;
            e_vNew_v2.Next = e2;

            vNew.IncidentEdge = e_vNew_v0;
            v0.IncidentEdge = e0;
            v1.IncidentEdge = e1;
            v2.IncidentEdge = e2;

            LegalizeEdge(e0, vNew);
            LegalizeEdge(e1, vNew);
            LegalizeEdge(e2, vNew);

            return _mesh;
        }

        public MeshInfo RemoveVertex(Vertex v)
        {
            throw new System.NotImplementedException();
        }

        private Face FindFaceContaining(Vector2 p)
        {
            Face currentFace = _mesh.Faces[0];
            bool found = false;

            var maxSteps = _mesh.Faces.Count * 10;
            int step = 0;
            while (!found && step < maxSteps)
            {
                step++;
                found = true;
                HalfEdge e = currentFace.Edge;

                for (int i = 0; i < 3; i++)
                {
                    Vector2 a = e.Origin.Position;
                    Vector2 b = e.Next.Origin.Position;

                    if (Geometry.Orientation(a, b, p) > 0)
                    {
                        if (e.Twin != null && e.Twin.Face != null)
                        {
                            currentFace = e.Twin.Face;
                            found = false;
                            break;
                        }
                    }
                    e = e.Next;
                }
            }
            return currentFace;
        }

        private void LegalizeEdge(HalfEdge e, Vertex vNew)
        {
            if (e.Twin == null || e.IsConstrained) return;

            Vertex vOpposite = e.Twin.Next.Next.Origin;

            Vertex a = e.Origin;
            Vertex b = e.Next.Origin;

            if (Geometry.InCircle(a.Position, b.Position, e.Next.Next.Origin.Position, vOpposite.Position))
            {
                FlipEdge(e);

                LegalizeEdge(e.Next, vNew);
                LegalizeEdge(e.Twin.Next.Next, vNew);
            }
        }

        private void FlipEdge(HalfEdge e)
        {
            HalfEdge t = e.Twin;
            if (t == null || e.IsConstrained) return;

            Face fA = e.Face;
            Face fB = t.Face;

            HalfEdge e_next = e.Next;
            HalfEdge e_nn = e_next.Next;

            HalfEdge t_next = t.Next;
            HalfEdge t_nn = t_next.Next;

            Vertex a = e.Origin;
            Vertex b = t.Origin;
            Vertex c = e_nn.Origin;
            Vertex d = t_nn.Origin;

            e.Origin = c;
            t.Origin = d;

            e.Next = t_nn;
            t_nn.Next = e_next;
            e_next.Next = e;

            t.Next = e_nn;
            e_nn.Next = t_next;
            t_next.Next = t;

            t_nn.Face = fA;
            e_nn.Face = fB;

            fA.Edge = e;
            fB.Edge = t;

            if (a.IncidentEdge == e) a.IncidentEdge = t_next;
            if (b.IncidentEdge == t) b.IncidentEdge = e_next;

            c.IncidentEdge = e;
            d.IncidentEdge = t;
        }
    }
}

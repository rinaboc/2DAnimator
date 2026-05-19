using System;
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
            HalfEdge e = FindEdgeContaining(point);
            if (e != null)
            {
                AddVertexOnEdge(point, e);
                return _mesh;
            }

            Face f0 = FindFaceContaining(point);
            if (f0 == null) return null;

            AddVertexOnFace(point, f0);

            return _mesh;
        }

        private void AddVertexOnFace(Vector2 point, Face f0)
        {
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
        }

        private void AddVertexOnEdge(Vector2 point, HalfEdge edgeToSplit)
        {
            bool constrained = edgeToSplit.IsConstrained;

            HalfEdge e_twin = edgeToSplit.Twin;
            HalfEdge e_AC = edgeToSplit.Next;
            HalfEdge e_CB = e_AC.Next;
            HalfEdge e_BD = e_twin?.Next;
            HalfEdge e_DA = e_BD?.Next;

            Vertex vA = edgeToSplit.Next.Origin;
            Vertex vB = edgeToSplit.Origin;
            Vertex vC = edgeToSplit.Next.Next.Origin;
            Vertex vD = e_twin?.Next.Next.Origin;

            Vertex vNew = new() { Position = point, UV = Vector2.zero };
            _mesh.Vertices.Add(vNew);

            Face fA = new();
            Face fD = edgeToSplit.Face;
            Face fC = e_twin?.Face;
            Face fB = e_twin == null ? null : new();

            _mesh.Faces.Add(fA);
            if (fB != null) _mesh.Faces.Add(fB);

            HalfEdge vNew_vC = new();
            HalfEdge vC_VNew = new();
            HalfEdge vNew_vD = e_twin == null ? null : new();
            HalfEdge vD_vNew = e_twin == null ? null : new();
            HalfEdge vA_vNew = e_twin == null ? null : new();
            HalfEdge vB_vNew = new();

            _mesh.HalfEdges.AddRange(new HalfEdge[] { vNew_vC, vC_VNew, vB_vNew });
            if (e_twin != null)
            {
                _mesh.HalfEdges.AddRange(new HalfEdge[] { vNew_vD, vD_vNew, vA_vNew });

                vA_vNew.Origin = vA;
                vA_vNew.Twin = edgeToSplit;
                vA_vNew.Face = fC;
                vA_vNew.Next = vNew_vD;
                vA_vNew.IsConstrained = constrained;

                e_twin.Origin = vNew;
                e_twin.Twin = vB_vNew;
                e_twin.Face = fB;

                vNew_vD.Origin = vNew;
                vNew_vD.Twin = vD_vNew;
                vNew_vD.Face = fC;
                vNew_vD.Next = e_DA;

                vD_vNew.Origin = vD;
                vD_vNew.Twin = vNew_vD;
                vD_vNew.Face = fB;
                vD_vNew.Next = e_twin;

                e_BD.Face = fB;
                e_BD.Next = vD_vNew;

                e_DA.Face = fC;
                e_DA.Next = vA_vNew;

                fC.Edge = e_DA;
                fB.Edge = e_BD;

                vD.IncidentEdge = e_DA;
            }

            edgeToSplit.Origin = vNew;
            edgeToSplit.Face = fD;
            edgeToSplit.Twin = e_twin != null ? vA_vNew : null;

            vB_vNew.Origin = vB;
            vB_vNew.Twin = e_twin;
            vB_vNew.Face = fA;
            vB_vNew.Next = vNew_vC;
            vB_vNew.IsConstrained = constrained;

            vNew_vC.Origin = vNew;
            vNew_vC.Twin = vC_VNew;
            vNew_vC.Face = fA;
            vNew_vC.Next = e_CB;

            vC_VNew.Origin = vC;
            vC_VNew.Next = edgeToSplit;
            vC_VNew.Face = fD;
            vC_VNew.Twin = vNew_vC;

            e_AC.Face = fD;
            e_AC.Next = vC_VNew;

            e_CB.Face = fA;
            e_CB.Next = vB_vNew;

            fD.Edge = e_AC;
            fA.Edge = e_CB;

            vA.IncidentEdge = e_AC;
            vC.IncidentEdge = e_CB;
            vB.IncidentEdge = e_twin != null ? e_BD : vB_vNew;

            vNew.IncidentEdge = vNew_vC;

            LegalizeEdge(e_AC, vNew);
            LegalizeEdge(e_CB, vNew);

            if (e_twin != null)
            {
                LegalizeEdge(e_BD, vNew);
                LegalizeEdge(e_DA, vNew);
            }
        }

        public MeshInfo RemoveVertex(Vertex v)
        {
            throw new System.NotImplementedException();
        }

        private HalfEdge FindEdgeContaining(Vector2 p)
        {
            foreach (var edge in _mesh.HalfEdges)
            {
                Vector2 ep = edge.Origin.Position;
                Vector2 enp = edge.Next.Origin.Position;

                float d = Geometry.Orientation(ep, enp, p);
                if (Math.Abs(d) > 0.1) continue;

                if (Math.Min(ep.x, enp.x) - 0.1f < p.x && Math.Max(ep.x, enp.x) + 0.1f > p.x &&
                    Math.Min(ep.y, enp.y) - 0.1f < p.y && Math.Max(ep.y, enp.y) + 0.1f > p.y
                )
                {
                    return edge;
                }
            }

            return null;
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

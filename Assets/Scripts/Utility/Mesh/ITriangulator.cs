using Assets.Scripts.Data.MeshInfo;
using UnityEngine;

namespace Assets.Scripts.Utility.Mesh
{
    public interface ITriangulator
    {
        MeshInfo AddVertex(Vector2 point);
        MeshInfo RemoveVertex(Vertex v);
    }
}

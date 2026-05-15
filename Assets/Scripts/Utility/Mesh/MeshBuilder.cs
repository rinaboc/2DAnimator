using System.Collections.Generic;
using Assets.Scripts.Data.MeshInfo;
using UnityEngine;

public class MeshBuilder : ManagerBase<MeshBuilder>
{
    [SerializeField] private Material[] Materials;
    public static GameObject Build(Texture2D texture, out MeshInfo meshInfo)
    {
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), pixelsPerUnit: 100, extrude: 1, meshType: SpriteMeshType.Tight);

        Mesh mesh = new()
        {
            vertices = System.Array.ConvertAll(sprite.vertices, v => (Vector3)v),
            triangles = System.Array.ConvertAll(sprite.triangles, t => (int)t),
            uv = sprite.uv
        };

        GameObject ArtMeshObject = new("ArtMesh")
        {
            layer = LayerMask.NameToLayer("UI")
        };
        ArtMeshObject.AddComponent<MeshFilter>().mesh = mesh;

        List<Material> mats = new();
        for (int i = 0; i < 1; i++)
        {
            Material material = Instance.Materials[i];
            Material _mat = new(material)
            {
                mainTexture = sprite.texture
            };
            _mat.SetVector("_TopLeftAnchor", ViewportManager.Instance.TopLeftAnchor);
            _mat.SetVector("_BottomRightAnchor", ViewportManager.Instance.BottomRightAnchor);
            mats.Add(_mat);
        }
        ArtMeshObject.AddComponent<MeshRenderer>().materials = mats.ToArray();

        BoxCollider boxCollider = ArtMeshObject.AddComponent<BoxCollider>();

        // fix offset
        // Vector3[] vertices = mesh.vertices;
        // for (int i = 0; i < vertices.Length; i++)
        // {
        //     vertices[i] -= boxCollider.center;
        //     vertices[i].z = 0f;
        // }
        // mesh.vertices = vertices;
        // ArtMeshObject.GetComponent<MeshFilter>().mesh = mesh;
        // boxCollider.center = Vector3.zero;

        TopologyBuilder.Build(mesh.vertices, mesh.uv, mesh.triangles, out meshInfo);

        return ArtMeshObject;
    }

    public static GameObject Build(Texture2D texture, MeshInfo meshInfo, out Vector2 min, out Vector2 size)
    {
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), pixelsPerUnit: 100, extrude: 1, meshType: SpriteMeshType.Tight);
        min = sprite.bounds.min;
        size = sprite.bounds.size;

        Mesh mesh = new();

        RebuildMesh(mesh, meshInfo);

        GameObject ArtMeshObject = new("ArtMesh")
        {
            layer = LayerMask.NameToLayer("UI")
        };
        ArtMeshObject.AddComponent<MeshFilter>().mesh = mesh;

        List<Material> mats = new();
        for (int i = 0; i < 1; i++)
        {
            Material material = Instance.Materials[i];
            Material _mat = new(material)
            {
                mainTexture = sprite.texture
            };
            _mat.SetVector("_TopLeftAnchor", ViewportManager.Instance.TopLeftAnchor);
            _mat.SetVector("_BottomRightAnchor", ViewportManager.Instance.BottomRightAnchor);
            mats.Add(_mat);
        }
        ArtMeshObject.AddComponent<MeshRenderer>().materials = mats.ToArray();
        ArtMeshObject.AddComponent<BoxCollider>();

        return ArtMeshObject;
    }

    public static GameObject Build(Texture2D texture, MeshInfo meshInfo)
    {
        var obj = Build(texture, meshInfo, out _, out _);
        return obj;
    }

    static void RebuildMesh(Mesh mesh, MeshInfo meshInfo)
    {
        int vertexCount = meshInfo.Vertices.Count;
        Vector3[] vert = new Vector3[vertexCount];
        Vector2[] uvs = new Vector2[vertexCount];
        Dictionary<Vertex, int> vertexToIndex = new();

        for (int i = 0; i < vertexCount; i++)
        {
            var vNode = meshInfo.Vertices[i];
            vert[i] = vNode.Position;
            uvs[i] = vNode.UV;
            vertexToIndex[vNode] = i;
        }

        int[] tris = new int[meshInfo.Faces.Count * 3];
        int triIndex = 0;

        foreach (var face in meshInfo.Faces)
        {
            HalfEdge e1 = face.Edge;
            HalfEdge e2 = e1.Next;
            HalfEdge e3 = e2.Next;

            tris[triIndex++] = vertexToIndex[e1.Origin];
            tris[triIndex++] = vertexToIndex[e2.Origin];
            tris[triIndex++] = vertexToIndex[e3.Origin];
        }

        mesh.Clear();
        mesh.vertices = vert;
        mesh.triangles = tris;
        mesh.uv = uvs;
        mesh.RecalculateBounds();
    }
}

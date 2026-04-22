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
        Vector3[] vertices = mesh.vertices;
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] -= boxCollider.center;
            vertices[i].z = 0f;
        }
        mesh.vertices = vertices;
        ArtMeshObject.GetComponent<MeshFilter>().mesh = mesh;
        boxCollider.center = Vector3.zero;

        TopologyBuilder.Build(mesh.vertices, mesh.triangles, out meshInfo);

        return ArtMeshObject;
    }
}

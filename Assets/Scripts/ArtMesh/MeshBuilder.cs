using UnityEngine;

public class MeshBuilder
{
    public static GameObject Build(Transform parentTransform, Material artMeshMaterial, Texture2D texture)
    {
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), pixelsPerUnit: 100, extrude: 1, meshType: SpriteMeshType.Tight);

        Mesh mesh = new()
        {
            vertices = System.Array.ConvertAll(sprite.vertices, v => (Vector3)v),
            triangles = System.Array.ConvertAll(sprite.triangles, t => (int)t),
            uv = sprite.uv
        };

        GameObject ArtMeshObject = new("ArtMesh");
        ArtMeshObject.transform.parent = parentTransform;
        ArtMeshObject.transform.localPosition = Vector3.zero;
        ArtMeshObject.transform.localScale = Vector3.one;
        ArtMeshObject.layer = LayerMask.NameToLayer("UI");
        ArtMeshObject.AddComponent<MeshFilter>().mesh = mesh;

        Material _meshMaterial = new(artMeshMaterial);
        _meshMaterial.mainTexture = sprite.texture;
        _meshMaterial.SetVector("_TopLeftAnchor", ViewportManager.instance.TopLeftAnchor);
        _meshMaterial.SetVector("_BottomRightAnchor", ViewportManager.instance.BottomRightAnchor);
        ArtMeshObject.AddComponent<MeshRenderer>().material = _meshMaterial;

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

        return ArtMeshObject;
    }
}

using Assets.Scripts.Data.MeshInfo;
using UnityEngine;

public class MeshVisualizer : ManagerBase<MeshVisualizer>
{
    public MeshInfo meshInfo;
    public Transform meshTransform;
    [SerializeField] private Material lineMaterial;

    void Start()
    {
        CreateLineMaterial();
    }
    public float lineThickness = 0.05f; // Adjust this value for desired thickness

    void OnRenderObject()
    {
        if (meshInfo == null || meshTransform == null) return;
        lineMaterial.SetPass(0);

        GL.PushMatrix();
        GL.MultMatrix(meshTransform.localToWorldMatrix);

        // Change to QUADS to allow for thickness
        GL.Begin(GL.QUADS);

        foreach (var edge in meshInfo.HalfEdges)
        {
            Vector2 a = edge.Origin.Position;
            Vector2 b = edge.Next.Origin.Position;

            // 1. Calculate direction and the perpendicular normal
            Vector2 dir = (b - a).normalized;
            Vector2 normal = new Vector2(-dir.y, dir.x); // Perpendicular vector

            // 2. Calculate the offset based on thickness
            Vector2 offset = normal * (lineThickness * 0.5f);

            // 3. Define the 4 corners of the quad
            GL.Color(edge.IsConstrained ? Color.red : Color.cyan);

            GL.Vertex(a + offset); // Top-left
            GL.Vertex(a - offset); // Bottom-left
            GL.Vertex(b - offset); // Bottom-right
            GL.Vertex(b + offset); // Top-right
        }

        GL.End();
        GL.PopMatrix();
    }

    void CreateLineMaterial()
    {
        if (!lineMaterial)
        {
            // Unity beépített egyszerű shadere vonalrajzoláshoz
            Shader shader = Shader.Find("Hidden/Internal-Colored");
            lineMaterial = new Material(shader);
            lineMaterial.hideFlags = HideFlags.HideAndDontSave;
            // Beállíthatod a blend módot, mélységtesztet stb.
            lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            lineMaterial.SetInt("_ZWrite", 0); // Ne takarja el a későbbi vonalakat
        }
    }
}

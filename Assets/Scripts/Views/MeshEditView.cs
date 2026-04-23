using Assets.Scripts.Data.MeshInfo;
using Assets.Scripts.States.EditMode;
using Assets.Scripts.Utility.MVI;
using UnityEngine;

public class MeshEditView : MonoBehaviour, IView<MeshUIEditState, MeshEditState>
{
    public GameObject ArtMeshObject { get; private set; }
    [SerializeField] private Material LineMaterial;
    [SerializeField] private float lineThickness = 0.03f;
    [SerializeField] private float vertexSize = 0.1f;

    private float scaledLineThickness, scaledVertexSize;

    private GameObject visualizer;
    private MeshInfo currentMeshInfo = null;

    private IViewModel<MeshUIEditState, MeshEditState> _viewModel;

    void Start()
    {
        if (!LineMaterial)
        {
            Shader shader = Shader.Find("Hidden/Internal-Colored");
            LineMaterial = new Material(shader);
            LineMaterial.hideFlags = HideFlags.HideAndDontSave;
            LineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            LineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            LineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            LineMaterial.SetInt("_ZWrite", 0);
        }


        ViewportEvents.ScaleChangeEvent.AddListener(OnScaleChanged);
        OnScaleChanged();
    }

    void OnScaleChanged()
    {
        float inverseScale = ScaleManager.OriginalScale / ScaleManager.Instance.CurrentScale;
        scaledVertexSize = vertexSize * inverseScale;
        scaledLineThickness = lineThickness * inverseScale;
    }

    void OnDestroy()
    {
        ViewportEvents.ScaleChangeEvent.RemoveListener(OnScaleChanged);
        _viewModel?.Unbind(this);
    }

    public MeshEditView SetArtMeshObject(GameObject artMeshObject)
    {
        ArtMeshObject = artMeshObject;
        ArtMeshObject.transform.parent = transform;
        ArtMeshObject.transform.localPosition = Vector3.zero;
        ArtMeshObject.transform.localScale = Vector3.one;

        ViewportManager viewportManager = ViewportManager.Instance;
        Vector3 topLeft = viewportManager.TopLeftAnchor;
        Vector3 bottomRight = viewportManager.BottomRightAnchor;

        Material[] meshMaterial = ArtMeshObject.GetComponent<MeshRenderer>().materials;
        meshMaterial[0].SetVector("_TopLeftAnchor", topLeft);
        meshMaterial[0].SetVector("_BottomRightAnchor", bottomRight);

        visualizer = new();
        visualizer.transform.parent = transform;
        visualizer.transform.localPosition = Vector3.zero;
        visualizer.transform.localScale = Vector3.one;
        visualizer.name = "Visualizer";

        return this;
    }

    public void Render(MeshEditState state)
    {
        if (state.Topology.Equals(currentMeshInfo)) return;

        currentMeshInfo = state.Topology;
    }

    void OnRenderObject()
    {
        LineMaterial.SetPass(0);

        GL.PushMatrix();
        GL.MultMatrix(visualizer.transform.localToWorldMatrix);

        GL.Begin(GL.QUADS);

        foreach (var edge in currentMeshInfo.HalfEdges)
        {
            Vector2 a = edge.Origin.Position;
            Vector2 b = edge.Next.Origin.Position;

            Vector2 dir = (b - a).normalized;
            Vector2 normal = new(-dir.y, dir.x);

            Vector2 offset = normal * (scaledLineThickness * 0.5f);

            GL.Color(edge.IsConstrained ? Color.red : Color.cyan * 0.8f);

            GL.Vertex(a + offset);
            GL.Vertex(a - offset);
            GL.Vertex(b - offset);
            GL.Vertex(b + offset);
        }

        float halfSize = scaledVertexSize * 0.5f;
        foreach (var vertex in currentMeshInfo.Vertices)
        {
            Vector2 p = vertex.Position;

            GL.Color(Color.black);

            GL.Vertex(new Vector3(p.x - halfSize, p.y - halfSize, 0));
            GL.Vertex(new Vector3(p.x - halfSize, p.y + halfSize, 0));
            GL.Vertex(new Vector3(p.x + halfSize, p.y + halfSize, 0));
            GL.Vertex(new Vector3(p.x + halfSize, p.y - halfSize, 0));
        }

        GL.End();
        GL.PopMatrix();
    }

    public void SetViewModel(IViewModel<MeshUIEditState, MeshEditState> viewModel)
    {
        _viewModel = viewModel;
        _viewModel?.Bind(this);
    }
}

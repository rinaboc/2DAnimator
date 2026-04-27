using System;
using Assets.Scripts.Data.MeshInfo;
using Assets.Scripts.States.EditMode;
using Assets.Scripts.Utility.MVI;
using UnityEngine;
using UnityEngine.InputSystem;

public class MeshEditView : DraggableHandle, IView<MeshUIEditState, MeshEditState>
{
    public GameObject ArtMeshObject { get; private set; }
    [SerializeField] private Material LineMaterial;
    [SerializeField] private float lineThickness = 0.03f;
    [SerializeField] private float vertexSize = 0.1f;

    private float scaledLineThickness, scaledVertexSize;

    private GameObject visualizer;
    private MeshInfo currentMeshInfo = null;
    private Vertex selectedVertex = null;
    private bool isDragging = false;

    private IViewModel<MeshUIEditState, MeshEditState> _viewModel;

    override protected void Start()
    {
        base.Start();

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

        boxCollider = ArtMeshObject.GetComponents<Collider>();
        ParentTransform = transform;
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
        var collider = ArtMeshObject.GetComponent<BoxCollider>();

        ViewportManager viewportManager = ViewportManager.Instance;
        Vector3 topLeft = viewportManager.TopLeftAnchor;
        Vector3 bottomRight = viewportManager.BottomRightAnchor;

        collider.size = new Vector3(Math.Abs(bottomRight.x - topLeft.x), Math.Abs(bottomRight.y - topLeft.y), 0);

        Material[] meshMaterial = ArtMeshObject.GetComponent<MeshRenderer>().materials;
        meshMaterial[0].SetVector("_TopLeftAnchor", topLeft);
        meshMaterial[0].SetVector("_BottomRightAnchor", bottomRight);

        visualizer = new();
        visualizer.transform.parent = ArtMeshObject.transform;
        visualizer.transform.localPosition = Vector3.zero;
        visualizer.transform.localScale = Vector3.one;
        visualizer.name = "Visualizer";

        return this;
    }

    public void Render(MeshEditState state)
    {
        selectedVertex = state.SelectedVertex;

        if (state.CurrentTopology == null || state.CurrentTopology.Equals(currentMeshInfo)) return;
        currentMeshInfo = state.CurrentTopology;

        var mesh = ArtMeshObject.GetComponent<MeshFilter>().mesh;
        visualizer.transform.localPosition = -mesh.bounds.center;
    }

    void OnRenderObject()
    {
        if (currentMeshInfo == null) return;

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

            if (vertex == selectedVertex) GL.Color(Color.yellow);
            else GL.Color(Color.black);

            GL.Vertex(new Vector3(p.x - halfSize, p.y - halfSize, 0));
            GL.Vertex(new Vector3(p.x - halfSize, p.y + halfSize, 0));
            GL.Vertex(new Vector3(p.x + halfSize, p.y + halfSize, 0));
            GL.Vertex(new Vector3(p.x + halfSize, p.y - halfSize, 0));
        }

        {
            GL.Color(Color.red);
            Vector2 p = Vector2.zero;
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

    protected override void OnClickStarted(InputAction.CallbackContext context)
    {
        if (IsInsideCollider())
        {
            Vector3 ClickLocalPos = visualizer.transform.InverseTransformPoint(ClickWorldPosition);
            _viewModel?.Send(new EditSelectVertexIntent(ClickLocalPos));
            isDragging = true;
        }
    }

    void Update()
    {
        if (isDragging)
        {
            Vector3 ClickLocalPos = visualizer.transform.InverseTransformPoint(ClickWorldPosition);
            _viewModel?.Send(new EditMoveVertexIntent(ClickLocalPos));
        }
    }

    protected override void OnDragFinished(InputAction.CallbackContext context)
    {
        if (isDragging) _viewModel?.Send(new EditMoveVertexEndedIntent());
        isDragging = false;
    }
}

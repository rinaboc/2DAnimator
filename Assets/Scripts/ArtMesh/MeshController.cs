using System;
using Assets.Scripts.ArtMesh;
using Assets.Scripts.Utility;
using UnityEngine;

public class MeshController : MonoBehaviour, ISelectable, IView<MeshState>
{
    public GameObject ArtMeshObject { get; private set; }
    [SerializeField] private Material ArtMeshMaterial;
    [SerializeField] private BoundingBox BoundingBox;
    public Guid ID { get; private set; }

    private Action<IIntent> EmitIntent;

    private void Awake()
    {
        if (ArtMeshMaterial == null || BoundingBox == null)
            Debug.LogError("Not all fields have been assigned.");
    }

    void Start()
    {
        if (MeshRegistry.Instance.TryGet(ID, out MeshData meshData))
            SetDrawOrder(meshData.drawOrder);
    }

    void OnEnable()
    {
        UIEvents.LayerSelectEvent += OnSelect;
        UIEvents.LayerDeselectEvent += OnDeselect;
    }

    void OnDestroy()
    {
        UIEvents.LayerSelectEvent -= OnSelect;
        UIEvents.LayerDeselectEvent -= OnDeselect;
    }

    public MeshController SetMeshID(Guid id)
    {
        ID = id;
        return this;
    }

    /// <summary>
    /// Update the two corner points that define a rectangular clipping area for drawing the ArtMesh.
    /// </summary>
    /// <returns></returns>
    public MeshController UpdateClipAnchors()
    {
        ViewportManager viewportManager = ViewportManager.Instance;

        Material _meshMaterial = ArtMeshObject.GetComponent<MeshRenderer>().material;

        if (_meshMaterial != null)
        {
            _meshMaterial.SetVector("_TopLeftAnchor", viewportManager.TopLeftAnchor);
            _meshMaterial.SetVector("_BottomRightAnchor", viewportManager.BottomRightAnchor);
        }

        BoundingBox.SetClipArea();

        return this;
    }

    public void SwapDrawOrder(MeshController swap)
    {
        MeshRegistry.Instance.TryGet(swap.ID, out MeshData swapMeshData);
        ushort newDrawOrder = swapMeshData.drawOrder;
        MeshRegistry.Instance.TryGet(ID, out MeshData thisMeshData);
        swap.SetDrawOrder(thisMeshData.drawOrder);
        this.SetDrawOrder(newDrawOrder);

    }

    public MeshController SetDrawOrder(ushort newDrawOrder)
    {
        if (MeshRegistry.Instance.TryGet(ID, out MeshData meshData))
        {
            meshData.drawOrder = newDrawOrder;

            Material _meshMaterial = ArtMeshObject.GetComponent<MeshRenderer>().material;

            if (_meshMaterial != null)
            {
                _meshMaterial.renderQueue = 2000 + meshData.drawOrder;
            }
        }
        else
        {
            Debug.LogError("Couldn't find mesh data to change draw order.");
        }

        return this;
    }

    public MeshController LoadSprite(Texture2D texture)
    {
        // art mesh creation
        ArtMeshObject = MeshBuilder.Build(BoundingBox.transform, ArtMeshMaterial, texture);

        BoxCollider boxCollider = ArtMeshObject.GetComponent<BoxCollider>();
        BoundingBox.boxCollider = boxCollider;
        BoundingBox.CreateBoundingBox(boxCollider.center, boxCollider.size);

        return this;
    }

    public void LoadTransformationFromMeshData()
    {
        if (MeshRegistry.Instance.TryGet(ID, out MeshData meshData))
        {
            this.transform.localPosition = meshData.transform.Position;
            ArtMeshObject.transform.localScale = meshData.transform.Scale;
            UpdateBoundingBox();
            this.transform.localRotation = meshData.transform.Rotation;
        }
        else
        {
            Debug.LogError("Couldn't find mesh data to load transformation from.");
        }
    }

    private void UpdateBoundingBox()
    {
        BoxCollider boxCollider = ArtMeshObject.GetComponent<BoxCollider>();
        BoundingBox.CreateBoundingBox(boxCollider.center, Vector3.Scale(boxCollider.size, ArtMeshObject.transform.localScale));
    }

    public void ScaleArtMesh(Vector3 scale)
    {
        ArtMeshObject.transform.localScale = scale;
    }

    public void RotateArtMesh(Quaternion rotation)
    {
        this.transform.localRotation = rotation;
    }

    public void MoveArtMesh(Vector3 newPosition)
    {
        this.transform.localPosition = newPosition;
    }

    public void UpdateTransform(object value, TransformType type)
    {
        TransformData transform = new();

        switch (type)
        {
            case TransformType.POSITION:
                transform.Position = (Vector3)value;
                break;
            case TransformType.ROTATION:
                transform.Rotation = (Quaternion)value;
                break;
            case TransformType.SCALE:
                transform.Scale = (Vector3)value;
                break;
        }

        EmitIntent(new UpdateTransformIntent(transform, type));
    }

    public void OnDeselect()
    {
        SetSelected(false);
    }

    public void OnSelect(Guid id)
    {
        SetSelected(id == ID);
    }

    public void SetSelected(bool isSelected)
    {
        BoundingBox.SetSelected(isSelected);
    }

    /// <summary>
    /// Saves a mesh's transformation values into the appropriate data objects. 
    /// When there are parameter points assigned, 
    /// the mesh's transformation difference from the value is calculated and the animation data is updated.
    /// </summary>
    /// <param name="value">transformation's value</param>
    public void SaveTransform(TransformType type)
    {
        EmitIntent(new SaveTransformIntent(type));
    }

    public void Render(MeshState state)
    {
        TransformData transform = state.MeshTransform + state.AnimationTransform;
        if (state.IsInterpolated)
        {
            transform = state.InterpolatedTransform;
        }

        MoveArtMesh(transform.Position);
        RotateArtMesh(transform.Rotation);
        ScaleArtMesh(transform.Scale);
    }

    public void SetIntentEmitter(Action<IIntent> intentEmitter)
    {
        EmitIntent = intentEmitter;
    }

    public BoundingBox GetBoundingBox() => BoundingBox;

    public void SendResetInterpolationIntent()
    {
        EmitIntent?.Invoke(new ResetInterpolationIntent(ID));
    }
}

using System;
using Assets.Scripts.States;
using Assets.Scripts.Utility.MVI;
using UnityEngine;

public class MeshController : MonoBehaviour, IView<MeshLayerStates, MeshStates>
{
    public GameObject ArtMeshObject { get; private set; }
    [SerializeField] private BoundingBox BoundingBox;
    public Guid ID { get; private set; }

    private IViewModel<MeshLayerStates, MeshStates> _viewModel;

    private void Awake()
    {
        if (BoundingBox == null)
            Debug.LogError("Not all fields have been assigned.");
    }

    void OnDestroy()
    {
        _viewModel?.Unbind(this);
    }

    public MeshController SetMeshID(Guid id)
    {
        ID = id;
        BoundingBox.SetID(id);
        return this;
    }

    public MeshController SetDrawOrder(ushort newDrawOrder)
    {
        Material _meshMaterial = ArtMeshObject.GetComponent<MeshRenderer>().material;

        if (_meshMaterial != null)
        {
            _meshMaterial.renderQueue = 2000 + newDrawOrder;
        }

        return this;
    }

    public MeshController SetArtMeshObject(GameObject artMeshObject)
    {
        ArtMeshObject = artMeshObject;
        ArtMeshObject.transform.parent = BoundingBox.transform;
        ArtMeshObject.transform.localPosition = Vector3.zero;
        ArtMeshObject.transform.localScale = Vector3.one;

        BoxCollider boxCollider = ArtMeshObject.GetComponent<BoxCollider>();
        BoundingBox.boxCollider = boxCollider;
        BoundingBox.CreateBoundingBox(boxCollider.center, boxCollider.size);
        return this;
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

        _viewModel?.Send(new UpdateTransformIntent(ID, transform, type));
    }

    /// <summary>
    /// Saves a mesh's transformation values into the appropriate data objects. 
    /// When there are parameter points assigned, 
    /// the mesh's transformation difference from the value is calculated and the animation data is updated.
    /// </summary>
    /// <param name="value">transformation's value</param>
    public void SaveTransform(object value, TransformType type)
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

        _viewModel?.Send(new SaveTransformIntent(ID, transform, type));
    }

    public void Render(MeshStates state)
    {
        if (!state.Meshes.TryGetValue(ID, out var meshState)) return;

        gameObject.SetActive(meshState.IsActive);
        if (!meshState.IsActive) return;

        TransformData transform = meshState.MeshTransform + meshState.AnimationTransform;
        if (meshState.IsInterpolated)
        {
            transform = meshState.InterpolatedTransform;
        }

        MoveArtMesh(transform.Position);
        RotateArtMesh(transform.Rotation);
        ScaleArtMesh(transform.Scale);
        SetDrawOrder(meshState.DrawOrder);
    }

    public BoundingBox GetBoundingBox() => BoundingBox;

    public void SendResetInterpolationIntent()
    {
        _viewModel?.Send(new ResetInterpolationIntent(ID));
    }

    public void SetViewModel(IViewModel<MeshLayerStates, MeshStates> viewModel)
    {
        _viewModel = viewModel;
        _viewModel?.Bind(this);
    }
}

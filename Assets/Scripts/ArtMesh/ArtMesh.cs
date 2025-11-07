using System;
using UnityEngine;

public class ArtMesh : MonoBehaviour, ISelectable
{
    public GameObject ArtMeshObject { get; private set; }
    [SerializeField] private Material ArtMeshMaterial;
    [SerializeField] private BoundingBox BoundingBox;
    public Guid MeshID { get; private set; }

    private void Awake()
    {
        if (ArtMeshMaterial == null || BoundingBox == null)
            Debug.LogError("Not all fields have been assigned.");
    }

    void Start()
    {
        if (MeshRegistry.Instance.TryGet(MeshID, out MeshData meshData))
            SetDrawOrder(meshData.drawOrder);
    }

    public ArtMesh SetMeshID(Guid id)
    {
        MeshID = id;
        return this;
    }

    /// <summary>
    /// Update the two corner points that define a rectangular clipping area for drawing the ArtMesh.
    /// </summary>
    /// <returns></returns>
    public ArtMesh UpdateClipAnchors()
    {
        ViewportManager viewportManager = ViewportManager.instance;

        Material _meshMaterial = ArtMeshObject.GetComponent<MeshRenderer>().material;

        if (_meshMaterial != null)
        {
            _meshMaterial.SetVector("_TopLeftAnchor", viewportManager.TopLeftAnchor);
            _meshMaterial.SetVector("_BottomRightAnchor", viewportManager.BottomRightAnchor);
        }

        BoundingBox.SetClipArea();

        return this;
    }

    public void SwapDrawOrder(ArtMesh swap)
    {
        MeshRegistry.Instance.TryGet(swap.MeshID, out MeshData swapMeshData);
        ushort newDrawOrder = swapMeshData.drawOrder;
        MeshRegistry.Instance.TryGet(MeshID, out MeshData thisMeshData);
        swap.SetDrawOrder(thisMeshData.drawOrder);
        this.SetDrawOrder(newDrawOrder);

    }

    public ArtMesh SetDrawOrder(ushort newDrawOrder)
    {
        if (MeshRegistry.Instance.TryGet(MeshID, out MeshData meshData))
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

    public ArtMesh LoadSprite(Texture2D texture)
    {
        // art mesh creation
        ArtMeshObject = MeshBuilder.Build(BoundingBox.transform, ArtMeshMaterial, texture);

        BoxCollider boxCollider = ArtMeshObject.GetComponent<BoxCollider>();
        BoundingBox.CreateBoundingBox(boxCollider.center, boxCollider.size);

        return this;
    }

    public void LoadTransformationFromMeshData()
    {
        if (MeshRegistry.Instance.TryGet(MeshID, out MeshData meshData))
        {
            this.transform.position = meshData.Position;
            ArtMeshObject.transform.localScale = meshData.Scale;
            UpdateBoundingBox();
            this.transform.localRotation = meshData.Rotation;
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
        UpdateBoundingBox();
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
        switch (type)
        {
            case TransformType.POSITION:
                MoveArtMesh((Vector3)value);
                break;
            case TransformType.ROTATION:
                RotateArtMesh((Quaternion)value);
                break;
            case TransformType.SCALE:
                ScaleArtMesh((Vector3)value);
                break;
        }
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
        MeshRegistry.Instance.TryGet(MeshID, out MeshData meshData);

        bool areParametersAssigned = ParamCurveRegistry.Instance.GetAssignedParamIDsOfMesh(meshData.ID).Count > 0;

        object updatedAnimationData = null;
        switch (type)
        {
            case TransformType.POSITION:
                if (areParametersAssigned)
                    updatedAnimationData = this.transform.localPosition - meshData.Position;
                else
                    meshData.Position = this.transform.localPosition;
                break;
            case TransformType.ROTATION:
                if (areParametersAssigned)
                    updatedAnimationData = Quaternion.Inverse(meshData.Rotation) * this.transform.localRotation;
                else
                    meshData.Rotation = this.transform.localRotation;
                break;
            case TransformType.SCALE:
                if (areParametersAssigned)
                    updatedAnimationData = ArtMeshObject.transform.localScale - meshData.Scale;
                else
                    meshData.Scale = ArtMeshObject.transform.localScale;
                break;
        }

        if (updatedAnimationData != null)
        {
            ParameterManager.instance.UpdateAnimationData(updatedAnimationData, type, meshData.ID);
        }
        else
            Debug.Log("updated animation data is null");
    }


}

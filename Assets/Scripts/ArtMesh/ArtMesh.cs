using System;
using UnityEngine;

public class ArtMesh : MonoBehaviour, ISelectable
{
    public GameObject ArtMeshObject { get; private set; }
    [SerializeField] private Material ArtMeshMaterial;
    [SerializeField] private BoundingBox BoundingBox;
    public ushort MeshID { get; private set; }

    private void Awake()
    {
        if (ArtMeshMaterial == null || BoundingBox == null)
            Debug.LogError("Not all fields have been assigned.");
    }

    void Start()
    {
        SetDrawOrder(MeshRegistry.instance.GetMeshData(MeshID).drawOrder);
    }

    public ArtMesh SetMeshID(ushort id)
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
        ushort newDrawOrder = MeshRegistry.instance.GetMeshData(swap.MeshID).drawOrder;
        swap.SetDrawOrder(MeshRegistry.instance.GetMeshData(MeshID).drawOrder);
        this.SetDrawOrder(newDrawOrder);

    }

    public ArtMesh SetDrawOrder(ushort newDrawOrder)
    {
        try
        {
            MeshData meshData = MeshRegistry.instance.GetMeshData(MeshID);
            meshData.drawOrder = newDrawOrder;

            Material _meshMaterial = ArtMeshObject.GetComponent<MeshRenderer>().material;

            if (_meshMaterial != null)
            {
                _meshMaterial.renderQueue = 2000 + meshData.drawOrder;
            }
        }
        catch (Exception)
        {
            Debug.LogError("Couldn't change draw order of mesh data.");
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

    public void ScaleArtMesh(Vector3 currentScale, Vector3 scale)
    {
        ArtMeshObject.transform.localScale = Vector3.Scale(currentScale, scale);
        BoxCollider boxCollider = ArtMeshObject.GetComponent<BoxCollider>();
        BoundingBox.CreateBoundingBox(boxCollider.center, Vector3.Scale(boxCollider.size, ArtMeshObject.transform.localScale));
    }

    public void RotateArtMesh(Quaternion currentRotation, Quaternion rotation)
    {
        this.transform.localRotation = currentRotation * rotation;
    }

    public void SetSelected(bool isSelected)
    {
        BoundingBox.SetSelected(isSelected);
    }

    public void UpdateTransform(object value, TransformType type)
    {
        MeshData meshData = MeshRegistry.instance.GetMeshData(MeshID);

        bool areParametersAssigned = ParameterRegistry.instance.GetAssignedParamIDsOfMesh(meshData.ID).Count > 0;

        object updatedAnimationData = null;
        switch (type)
        {
            case TransformType.POSITION:
                if (areParametersAssigned)
                    updatedAnimationData = (Vector3)value - meshData.Position;
                else
                    meshData.Position = (Vector3)value;
                break;
            case TransformType.ROTATION:
                if (areParametersAssigned)
                    updatedAnimationData = Quaternion.Inverse(meshData.Rotation) * (Quaternion)value;
                else
                    meshData.Rotation = (Quaternion)value;
                break;
            case TransformType.SCALE:
                if (areParametersAssigned)
                    updatedAnimationData = (Vector3)value - meshData.Scale;
                else
                    meshData.Scale = (Vector3)value;
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

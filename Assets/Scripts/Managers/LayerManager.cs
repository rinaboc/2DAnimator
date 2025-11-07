using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System;

public class LayerManager : MonoBehaviour
{
    [Header("Art Mesh creation")]
    [SerializeField] private GameObject ArtObjectPrefab;
    [SerializeField] private GameObject ViewportScale;

    [Header("UI settings")]
    [SerializeField] private GameObject UIArtLayerPrefab;
    [SerializeField] private GameObject UILayersContent;

    private InputAction CancelAction;

    /// <summary>
    /// Currently selected mesh data's id
    /// </summary>
    private bool _isLayerSelected = false;
    private Guid _selectedLayerID;

    private GameObject SelectedUILayer
    {
        get
        {
            if (_isLayerSelected)
            {
                return MeshRegistry.Instance.GetUILayer(_selectedLayerID).ParentObj;
            }
            else return null;
        }
    }

    public ArtMesh SelectedArtMesh
    {
        get
        {
            if (_isLayerSelected)
            {
                return MeshRegistry.Instance.GetArtMesh(_selectedLayerID).GetComponent<ArtMesh>();
            }
            else return null;
        }
    }

    public static LayerManager instance;

    void Start()
    {
        CancelAction = InputSystem.actions.FindAction("Cancel");
        CancelAction.performed += OnCancel;
    }

    void OnDestroy()
    {
        CancelAction.Dispose();
    }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(this);
        }
    }

    private void OnCancel(InputAction.CallbackContext context)
    {
        DeselectCurrentUIArtLayer();
    }

    /// <summary>
    /// Create an ArtObject inside the viewport and assign the input texture as the sprite.
    /// </summary>
    public void CreateArtMesh(Texture2D texture, string path)
    {
        MeshData newMesh = new(path);
        MeshRegistry.Instance.RegisterMeshData(newMesh);

        // create ArtObject inside viewport and assign the image to its sprite
        GameObject newArtObject = Instantiate(ArtObjectPrefab, ViewportScale.transform, false);
        newArtObject.name = "ArtObject" + newMesh.ID;
        newArtObject.GetComponent<ArtMesh>()
            .LoadSprite(texture)
            .SetMeshID(newMesh.ID)
            .SetSelected(false);

        MeshRegistry.Instance.RegisterArtMeshObj(newMesh.ID, newArtObject);

        CreateUIArtLayer(newMesh);
        SelectUIArtLayer(newMesh.ID);
    }

    /// <summary>
    /// Creates a UI element to represent the ArtLayers in the project.
    /// </summary>
    private void CreateUIArtLayer(MeshData meshData)
    {
        GameObject newArtLayer = Instantiate(UIArtLayerPrefab, UILayersContent.transform);
        newArtLayer.name = "ArtLayer" + meshData.ID;
        newArtLayer.transform.SetSiblingIndex(0);

        newArtLayer.GetComponentInChildren<LayerController>()
            .SetID(meshData.ID)
            .SetText(meshData.name)
            .SetSelected(false);

        newArtLayer.GetComponent<Button>().onClick.AddListener(() =>
        {
            SelectUIArtLayer(meshData.ID);
        });

        MeshRegistry.Instance.RegisterUILayer(meshData.ID, newArtLayer.GetComponentInChildren<LayerController>());
    }

    public void SelectUIArtLayer(Guid id)
    {
        // deselect previous layer
        DeselectCurrentUIArtLayer();

        // select the new layer
        _selectedLayerID = id;
        _isLayerSelected = true;
        SelectedUILayer
            .GetComponentInChildren<LayerController>()
            .SetSelected(true);
        SelectedArtMesh.SetSelected(true);

        ParameterManager.instance.HighlightCreatedCurves(SelectedArtMesh.MeshID);

    }

    public void DeselectCurrentUIArtLayer()
    {
        if (SelectedUILayer != null)
        {
            SelectedUILayer.GetComponentInChildren<LayerController>().SetSelected(false);
            SelectedArtMesh.SetSelected(false);
        }
    }

    public void MoveUIArtLayerUp()
    {
        if (!_isLayerSelected) return; // nothing is selected

        Transform selectedArtLayer = SelectedUILayer.transform;
        int artLayerIndex = selectedArtLayer.GetSiblingIndex();

        if (artLayerIndex > 0)
        {
            Guid swappedID = UILayersContent.transform.GetChild(artLayerIndex - 1).gameObject.GetComponentInChildren<LayerController>().LayerID;
            ArtMesh swappedMesh = MeshRegistry.Instance.GetArtMesh(swappedID).GetComponent<ArtMesh>();
            SelectedArtMesh.SwapDrawOrder(swappedMesh);

            selectedArtLayer.SetSiblingIndex(artLayerIndex - 1);

        }
    }

    public void MoveUIArtLayerDown()
    {
        if (!_isLayerSelected) return;

        Transform selectedArtLayer = SelectedUILayer.transform;
        int artLayerIndex = selectedArtLayer.GetSiblingIndex();

        if (artLayerIndex < UILayersContent.transform.childCount - 1)
        {
            Guid swappedID = UILayersContent.transform.GetChild(artLayerIndex + 1).gameObject.GetComponentInChildren<LayerController>().LayerID;
            ArtMesh swappedMesh = MeshRegistry.Instance.GetArtMesh(swappedID).GetComponent<ArtMesh>();
            SelectedArtMesh.SwapDrawOrder(swappedMesh);

            selectedArtLayer.SetSiblingIndex(artLayerIndex + 1);
        }
    }

    public void DeleteSelectedArtObject()
    {
        if (!_isLayerSelected) return;

        ParameterManager.instance.DeleteParamPointsOfMesh(SelectedArtMesh.MeshID);

        MeshRegistry.Instance.DeleteArtMeshObj(_selectedLayerID);
        MeshRegistry.Instance.DeleteUILayer(_selectedLayerID);
        MeshRegistry.Instance.DeleteMeshData(_selectedLayerID);

        // deselect
        _isLayerSelected = false;
    }
}

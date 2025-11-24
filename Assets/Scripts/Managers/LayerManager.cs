using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System;

/// <summary>
/// Manager for handling Art Mesh layers and their corresponding UI elements
/// </summary>
public class LayerManager : ManagerBase<LayerManager>
{
    [Header("UI settings")]
    [SerializeField] private GameObject UIArtLayerPrefab;
    [SerializeField] private GameObject UILayersContent;

    private Dictionary<Guid, LayerController> _layerControllers = new();

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
            if (_isLayerSelected && GetUILayer(_selectedLayerID, out LayerController layer))
            {
                return layer.ParentObj;
            }
            else return null;
        }
    }

    public MeshController SelectedArtMesh
    {
        get
        {
            if (_isLayerSelected && MeshManager.Instance.GetMeshObject(_selectedLayerID, out MeshController artMesh))
            {
                return artMesh;
            }
            else return null;
        }
    }

    void OnEnable()
    {
        CancelAction = InputSystem.actions.FindAction("Cancel");
        CancelAction.performed += OnCancel;
    }

    void OnDestroy()
    {
        CancelAction.performed -= OnCancel;
    }


    private void OnCancel(InputAction.CallbackContext context)
    {
        DeselectCurrentUIArtLayer();
    }

    /// <summary>
    /// Creates a UI element to represent the ArtLayers in the project.
    /// </summary>
    public void CreateUIArtLayer(MeshData meshData)
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

        RegisterUILayer(meshData.ID, newArtLayer.GetComponentInChildren<LayerController>());
        SelectUIArtLayer(meshData.ID);
    }

    public void SelectUIArtLayer(Guid id)
    {
        // deselect previous layer
        DeselectCurrentUIArtLayer();

        // select the new layer
        _selectedLayerID = id;
        _isLayerSelected = true;

        UIEvents.RaiseLayerSelect(id);
    }

    public void DeselectCurrentUIArtLayer()
    {
        if (_isLayerSelected)
        {
            UIEvents.RaiseLayerDeselect();
            // SelectedUILayer.GetComponentInChildren<LayerController>().SetSelected(false);
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
            Guid swappedID = UILayersContent.transform.GetChild(artLayerIndex - 1).gameObject.GetComponentInChildren<LayerController>().ID;
            MeshManager.Instance.GetMeshObject(swappedID, out MeshController swappedMesh);
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
            Guid swappedID = UILayersContent.transform.GetChild(artLayerIndex + 1).gameObject.GetComponentInChildren<LayerController>().ID;
            MeshManager.Instance.GetMeshObject(swappedID, out MeshController swappedMesh);
            SelectedArtMesh.SwapDrawOrder(swappedMesh);

            selectedArtLayer.SetSiblingIndex(artLayerIndex + 1);
        }
    }

    public void DeleteSelectedArtObject()
    {
        if (!_isLayerSelected) return;

        Guid meshID = SelectedArtMesh.ID;

        DeleteUILayer(meshID);

        // deselect
        _isLayerSelected = false;
    }

    private bool GetUILayer(Guid id, out LayerController layer) => _layerControllers.TryGetValue(id, out layer);

    private bool RegisterUILayer(Guid id, LayerController controller) => _layerControllers.TryAdd(id, controller);

    private bool DeleteUILayer(Guid id)
    {
        if (_layerControllers.TryGetValue(id, out LayerController layer))
        {
            _layerControllers.Remove(id);
            Destroy(layer.ParentObj);
            UIEvents.RaiseLayerDelete(id);
            return true;
        }

        return false;
    }

    public override void LoadState(SaveData saveData)
    {
        throw new NotImplementedException();
    }
}

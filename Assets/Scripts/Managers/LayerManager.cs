using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using System;
using System.Linq;
using Assets.Scripts.Utility.MVI;
using Assets.Scripts.States;

/// <summary>
/// Manager for handling Art Mesh layers and their corresponding UI elements
/// </summary>
public class LayerManager : ManagerBase<LayerManager>
{
    [Header("UI settings")]
    [SerializeField] private GameObject UIArtLayerPrefab;
    [SerializeField] private GameObject UILayersContent;

    private Dictionary<Guid, LayerController> _layerControllers = new();

    private IViewModel<LayerStates> _viewModel;

    private InputAction CancelAction;


    void Start()
    {
        _viewModel = ViewModelFactory.Instance.CreateViewModel<LayersViewModel, LayerStates>(gameObject);
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
        _viewModel?.Send(new SelectLayerIntent(Guid.Empty));
    }

    /// <summary>
    /// Creates a UI element to represent the ArtLayers in the project.
    /// </summary>
    public void CreateUIArtLayer(Guid ID)
    {
        GameObject newArtLayer = Instantiate(UIArtLayerPrefab, UILayersContent.transform);
        newArtLayer.name = "ArtLayer" + ID;
        newArtLayer.transform.SetSiblingIndex(0);

        var layerController = newArtLayer.GetComponentInChildren<LayerController>();
        layerController.SetID(ID);
        layerController.SetViewModel(_viewModel);

        _layerControllers.TryAdd(ID, layerController);
    }

    public void MoveUIArtLayerUp()
    {
        _viewModel?.Send(new MoveLayerUpIntent());
    }

    public void MoveLayerUp(Guid ID)
    {
        if (!GetUILayer(ID, out LayerController layer)) return;
        int layerIdx = layer.ParentObj.transform.GetSiblingIndex();
        if (layerIdx > 0)
        {
            layer.ParentObj.transform.SetSiblingIndex(layerIdx - 1);
        }
    }

    public void MoveUIArtLayerDown()
    {
        _viewModel?.Send(new MoveLayerDownIntent());
    }

    public void MoveLayerDown(Guid ID)
    {
        if (!GetUILayer(ID, out LayerController layer)) return;
        int layerIdx = layer.ParentObj.transform.GetSiblingIndex();
        if (layerIdx < UILayersContent.transform.childCount - 1)
        {
            layer.ParentObj.transform.SetSiblingIndex(layerIdx + 1);
        }
    }

    public void DeleteSelectedArtObject()
    {
        _viewModel?.Send(new DeleteLayerIntent());
    }

    private bool GetUILayer(Guid id, out LayerController layer) => _layerControllers.TryGetValue(id, out layer);

    public bool DeleteUILayer(Guid id)
    {
        if (_layerControllers.TryGetValue(id, out LayerController layer))
        {
            _layerControllers.Remove(id);
            Destroy(layer.ParentObj);
            return true;
        }

        return false;
    }

    public void DeleteAllUILayers()
    {
        foreach (var item in _layerControllers)
        {
            Destroy(item.Value.ParentObj);
        }
        _layerControllers.Clear();
    }
}

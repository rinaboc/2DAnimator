using System;
using System.Collections.Generic;
using Assets.Scripts.States;
using Assets.Scripts.States.EditMode;
using Assets.Scripts.Utility.MVI;
using UnityEngine;

public class MeshManager : ManagerBase<MeshManager>
{
    [SerializeField] private Dictionary<Guid, MeshController> _meshControllers = new();
    [SerializeField] private AppInitializer _appInitializer;

    [Header("Art Mesh creation")]
    [SerializeField] private GameObject ArtObjectPrefab;
    [SerializeField] private GameObject ViewportScale;
    private IViewModel<MeshLayerStates, MeshStates> _viewModel;

    [Header("Mesh Edit creation")]
    [SerializeField] private GameObject MeshEditPrefab;
    private IViewModel<MeshUIEditState, MeshEditState> _meshEditViewModel;
    private GameObject _meshEditObject;


    public bool GetMeshObject(Guid id, out MeshController artMesh) => _meshControllers.TryGetValue(id, out artMesh);

    void Start()
    {
        if (!_appInitializer.GetViewModel(out _viewModel))
        {
            Debug.LogError("Couldn't fetch viewModel");
        }

        if (!_appInitializer.GetViewModel(out _meshEditViewModel))
        {
            Debug.LogError("Couldn't fetch viewModel");
        }
    }

    public void DeleteArtMeshObj(Guid id)
    {
        MeshController artMesh = _meshControllers[id];
        _meshControllers.Remove(id);
        Destroy(artMesh.gameObject);
    }

    public void ClearArtMeshObjects()
    {
        foreach (var artMesh in _meshControllers.Values)
        {
            Destroy(artMesh.gameObject);
        }
        _meshControllers.Clear();
    }

    public void DeleteMeshEditObj()
    {
        Destroy(_meshEditObject);
    }


    public void CreateMeshEditObj(GameObject meshEditObject, Guid ID)
    {
        GameObject newMeshEditObject = Instantiate(MeshEditPrefab, ViewportScale.transform, false);
        newMeshEditObject.name = "MeshEdit" + ID;
        MeshEditView meshEditView = newMeshEditObject.GetComponent<MeshEditView>();
        meshEditView.SetArtMeshObject(meshEditObject);
        meshEditView.SetViewModel(_meshEditViewModel);
        _meshEditObject = newMeshEditObject;
    }

    public void CreateArtMeshObj(GameObject artMeshObject, Guid ID)
    {
        // create ArtObject inside viewport and assign the image to its sprite
        MeshController meshController = SetupMeshController(artMeshObject, ID);
        meshController.SetViewModel(_viewModel);
        meshController.GetBoundingBox().SetViewModel(_viewModel);

        _meshControllers.TryAdd(ID, meshController);
    }

    public void DispatchToMeshViewModel(IIntent intent)
    {
        _viewModel?.Send(intent);
    }

    public MeshController SetupMeshController(GameObject artMeshObject, Guid ID)
    {
        GameObject newArtObject = Instantiate(ArtObjectPrefab, ViewportScale.transform, false);
        newArtObject.name = "ArtObject" + ID;
        MeshController meshController = newArtObject.GetComponent<MeshController>();
        meshController
            .SetArtMeshObject(artMeshObject)
            .SetMeshID(ID);
        return meshController;
    }
}

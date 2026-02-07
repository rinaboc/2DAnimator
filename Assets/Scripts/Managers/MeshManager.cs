using System;
using System.Collections.Generic;
using Assets.Scripts.States;
using Assets.Scripts.Utility.MVI;
using UnityEngine;

public class MeshManager : ManagerBase<MeshManager>
{
    [SerializeField] private Dictionary<Guid, MeshController> _meshControllers = new();
    private IViewModel<MeshLayerStates, MeshStates> _viewModel;
    [SerializeField] private AppInitializer _appInitializer;

    [Header("Art Mesh creation")]
    [SerializeField] private GameObject ArtObjectPrefab;
    [SerializeField] private GameObject ViewportScale;

    public bool GetMeshObject(Guid id, out MeshController artMesh) => _meshControllers.TryGetValue(id, out artMesh);

    void Start()
    {
        if (!_appInitializer.GetViewModel(out _viewModel))
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

    public void CreateArtMeshObj(Texture2D texture, Guid ID)
    {
        // create ArtObject inside viewport and assign the image to its sprite
        MeshController meshController = SetupMeshController(texture, ID);
        meshController.SetViewModel(_viewModel);
        meshController.GetBoundingBox().SetViewModel(_viewModel);

        _meshControllers.TryAdd(ID, meshController);
    }

    public void DispatchToMeshViewModel(IIntent intent)
    {
        _viewModel?.Send(intent);
    }

    public MeshController SetupMeshController(Texture2D texture, Guid ID)
    {
        GameObject newArtObject = Instantiate(ArtObjectPrefab, ViewportScale.transform, false);
        newArtObject.name = "ArtObject" + ID;
        MeshController meshController = newArtObject.GetComponent<MeshController>();
        meshController
            .LoadSprite(texture)
            .SetMeshID(ID);
        return meshController;
    }
}

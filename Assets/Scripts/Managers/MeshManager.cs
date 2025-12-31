using System;
using System.Collections.Generic;
using Assets.Scripts.States;
using Assets.Scripts.Utility.MVI;
using UnityEngine;

public class MeshManager : ManagerBase<MeshManager>
{
    [SerializeField] private Dictionary<Guid, MeshController> _meshControllers = new();
    private Store<MeshStates> _store;
    private IViewModel<MeshStates> _viewModel;


    [Header("Art Mesh creation")]
    [SerializeField] private GameObject ArtObjectPrefab;
    [SerializeField] private GameObject ViewportScale;

    void Start()
    {
        _viewModel = ViewModelFactory.Instance.CreateViewModel<MeshViewModel, MeshStates>(gameObject);
        _store = ((MeshViewModel)_viewModel).GetStore();
    }

    public bool GetMeshObject(Guid id, out MeshController artMesh) => _meshControllers.TryGetValue(id, out artMesh);

    public bool RegisterArtMeshObj(Guid id, MeshController artMeshObj) => _meshControllers.TryAdd(id, artMeshObj);

    public void DeleteArtMeshObj(Guid id)
    {
        MeshController artMesh = _meshControllers[id];
        _meshControllers.Remove(id);
        Destroy(artMesh.gameObject);
    }

    public void CreateArtMeshObj(Texture2D texture, Guid ID)
    {
        // create ArtObject inside viewport and assign the image to its sprite
        MeshController meshController = SetupMeshController(texture, ID);

        meshController.SetViewModel(_viewModel);
        meshController.GetBoundingBox().SetViewModel(_viewModel);

        RegisterArtMeshObj(ID, meshController);
    }

    public void DispatchToMeshStore(IIntent intent)
    {
        _store.Dispatch(intent);
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


    public override void LoadState(SaveData saveData)
    {
        MeshRegistry.Instance.Clear();

        foreach (var item in saveData.MeshDatas)
        {
            if (NFPController.LoadImage(item.sourcePath, out Texture2D texture))
            {
                MeshRegistry.Instance.Register(item);
                MeshController meshController = SetupMeshController(texture, item.ID);
                meshController.LoadTransformationFromMeshData();
                RegisterArtMeshObj(item.ID, meshController);
            }
        }
    }
}

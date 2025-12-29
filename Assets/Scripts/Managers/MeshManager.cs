using System;
using System.Collections.Generic;
using Assets.Scripts.States;
using Assets.Scripts.Utility.MVI;
using UnityEngine;

public class MeshManager : ManagerBase<MeshManager>
{
    [SerializeField] private Dictionary<Guid, MeshController> _meshControllers = new();
    private Dictionary<Guid, Store<MeshState>> _meshStores = new();

    [SerializeField] private AppInitializer _appInitializer;


    [Header("Art Mesh creation")]
    [SerializeField] private GameObject ArtObjectPrefab;
    [SerializeField] private GameObject ViewportScale;

    override protected void Awake()
    {
        base.Awake();
    }

    void OnEnable()
    {
        UIEvents.LayerDeleteEvent += DeleteArtMeshObj;
    }

    void OnDisable()
    {
        UIEvents.LayerDeleteEvent -= DeleteArtMeshObj;
    }

    public bool GetMeshObject(Guid id, out MeshController artMesh) => _meshControllers.TryGetValue(id, out artMesh);

    public bool RegisterArtMeshObj(Guid id, MeshController artMeshObj) => _meshControllers.TryAdd(id, artMeshObj);

    public void DeleteArtMeshObj(Guid id)
    {
        MeshController artMesh = _meshControllers[id];
        _meshControllers.Remove(id);
        Destroy(artMesh.gameObject);

        MeshRegistry.Instance.Remove(id);
    }

    public MeshData CreateArtMeshObj(Texture2D texture, MeshData newMesh)
    {
        // create ArtObject inside viewport and assign the image to its sprite
        MeshController meshController = SetupMeshController(texture, newMesh);

        var viewModel = ViewModelFactory.Instance.CreateViewModel<MeshViewModel, MeshState>(meshController.gameObject, new MeshState()
        {
            ID = newMesh.ID,
            MeshTransform = newMesh.transform,
            AnimationTransform = new TransformData()
        });

        meshController.SetViewModel(viewModel);
        meshController.GetBoundingBox().SetViewModel(viewModel);

        _meshStores[newMesh.ID] = ((MeshViewModel)viewModel).GetStore();
        RegisterArtMeshObj(newMesh.ID, meshController);
        return newMesh;
    }

    public void DispatchToMeshStore(Guid meshId, IIntent intent)
    {
        if (_meshStores.TryGetValue(meshId, out Store<MeshState> store))
        {
            store.Dispatch(intent);
        }
        else
        {
            Debug.LogError($"No store found for mesh {meshId}");
        }
    }

    public MeshController SetupMeshController(Texture2D texture, MeshData newMesh)
    {
        GameObject newArtObject = Instantiate(ArtObjectPrefab, ViewportScale.transform, false);
        newArtObject.name = "ArtObject" + newMesh.ID;
        MeshController meshController = newArtObject.GetComponent<MeshController>();
        meshController
            .LoadSprite(texture)
            .SetMeshID(newMesh.ID)
            .SetDrawOrder(newMesh.drawOrder)
            .SetSelected(false);
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
                MeshController meshController = SetupMeshController(texture, item);
                meshController.LoadTransformationFromMeshData();
                RegisterArtMeshObj(item.ID, meshController);
            }
        }
    }
}

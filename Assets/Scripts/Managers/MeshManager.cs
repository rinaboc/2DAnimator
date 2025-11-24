using System;
using System.Collections.Generic;
using UnityEngine;

public class MeshManager : ManagerBase<MeshManager>
{
    [SerializeField] private Dictionary<Guid, MeshController> _meshControllers = new();

    [Header("Art Mesh creation")]
    [SerializeField] private GameObject ArtObjectPrefab;
    [SerializeField] private GameObject ViewportScale;

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

    public MeshData CreateArtMeshObj(Texture2D texture, string path)
    {
        MeshData newMesh = new(path);

        // create ArtObject inside viewport and assign the image to its sprite
        GameObject newArtObject = Instantiate(ArtObjectPrefab, ViewportScale.transform, false);
        newArtObject.name = "ArtObject" + newMesh.ID;
        newArtObject.GetComponent<MeshController>()
            .LoadSprite(texture)
            .SetMeshID(newMesh.ID)
            .SetSelected(false);

        RegisterArtMeshObj(newMesh.ID, newArtObject.GetComponent<MeshController>());
        return newMesh;
    }

    public override void LoadState(SaveData saveData)
    {
        throw new NotImplementedException();
    }
}

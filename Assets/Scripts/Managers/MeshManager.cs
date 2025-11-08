using System;
using System.Collections.Generic;
using UnityEngine;

public class MeshManager : ManagerBase<MeshManager>
{
    [SerializeField] private Dictionary<Guid, MeshController> _meshControllers = new();

    public bool GetMeshObject(Guid id, out MeshController artMesh) => _meshControllers.TryGetValue(id, out artMesh);

    public bool RegisterArtMeshObj(Guid id, MeshController artMeshObj) => _meshControllers.TryAdd(id, artMeshObj);

    public void DeleteArtMeshObj(Guid id)
    {
        MeshController artMesh = _meshControllers[id];
        _meshControllers.Remove(id);
        Destroy(artMesh.gameObject);
    }
}

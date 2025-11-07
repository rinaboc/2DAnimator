using System;
using System.Collections.Generic;
using UnityEngine;

public class MeshManager : MonoBehaviour
{
    [SerializeField] private Dictionary<Guid, GameObject> _artMeshes = new();
    public static MeshManager Instance;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(this);
        }
    }

    public GameObject GetArtMesh(Guid id)
    {
        if (_artMeshes.ContainsKey(id))
        {
            return _artMeshes[id];
        }
        else
        {
            Debug.LogError("no such id in registry");
            return null;
        }
    }

    public void RegisterArtMeshObj(Guid id, GameObject artMeshObj)
    {
        if (_artMeshes.ContainsKey(id))
        {
            Debug.LogError("duplicate id in registry");
        }

        _artMeshes.Add(id, artMeshObj);
    }

    public void DeleteArtMeshObj(Guid id)
    {
        GameObject artMesh = _artMeshes[id];
        _artMeshes.Remove(id);
        Destroy(artMesh);
    }
}

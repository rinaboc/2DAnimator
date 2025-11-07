using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MeshRegistry", menuName = "Global/Mesh Registry")]

public class MeshRegistry : ScriptableObject
{
    [SerializeField] private Dictionary<Guid, GameObject> _artMeshes = new();
    [SerializeField] private Dictionary<Guid, LayerController> _layerControllers = new();
    [SerializeField] private Dictionary<Guid, MeshData> _meshDataEntries = new();

    private static MeshRegistry _instance;
    public static MeshRegistry Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<MeshRegistry>("MeshRegistry");

                if (_instance == null)
                {
                    Debug.LogError("MeshRegistry asset not found in Resources!");
                }
            }

            return _instance;
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

    public LayerController GetUILayer(Guid id)
    {
        if (_layerControllers.ContainsKey(id))
        {
            return _layerControllers[id];
        }
        else
        {
            Debug.LogError("no such id in registry");
            return null;
        }
    }

    public void RegisterUILayer(Guid id, LayerController controller)
    {
        if (_layerControllers.ContainsKey(id))
        {
            Debug.LogError("duplicate id in registry");
        }

        _layerControllers.Add(id, controller);
    }

    public void DeleteUILayer(Guid id)
    {
        GameObject uilayer = _layerControllers[id].ParentObj;
        _layerControllers.Remove(id);
        Destroy(uilayer);
    }

    public MeshData GetMeshData(Guid id)
    {
        try
        {
            return _meshDataEntries[id];
        }
        catch (Exception e)
        {
            Debug.LogError("Couldn't fetch MeshData");
            throw e;
        }
    }

    public void UpdateMeshData(MeshData meshData) => _meshDataEntries[meshData.ID] = meshData;

    public void RegisterMeshData(MeshData meshData)
    {
        if (_meshDataEntries.ContainsKey(meshData.ID))
        {
            Debug.LogError("duplicate id in registry");
        }

        _meshDataEntries.Add(meshData.ID, meshData);
    }

    public void DeleteMeshData(Guid id) => _meshDataEntries.Remove(id);
}

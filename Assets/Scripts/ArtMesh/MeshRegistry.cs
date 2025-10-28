using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MeshRegistry", menuName = "Global/Mesh Registry")]

public class MeshRegistry : ScriptableObject
{
    [SerializeField, HideInInspector]
    private Dictionary<Guid, GameObject> ArtMeshes = new();
    [SerializeField, HideInInspector]
    private Dictionary<Guid, LayerInteractionController> UILayers = new();
    [SerializeField, HideInInspector]
    private Dictionary<Guid, MeshData> MeshDataEntries = new();

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
        if (ArtMeshes.ContainsKey(id))
        {
            return ArtMeshes[id];
        }
        else
        {
            Debug.LogError("no such id in registry");
            return null;
        }
    }

    public void RegisterArtMeshObj(Guid id, GameObject artMeshObj)
    {
        if (ArtMeshes.ContainsKey(id))
        {
            Debug.LogError("duplicate id in registry");
        }

        ArtMeshes.Add(id, artMeshObj);
    }

    public void DeleteArtMeshObj(Guid id)
    {
        GameObject artMesh = ArtMeshes[id];
        ArtMeshes.Remove(id);
        Destroy(artMesh);
    }

    public LayerInteractionController GetUILayer(Guid id)
    {
        if (UILayers.ContainsKey(id))
        {
            return UILayers[id];
        }
        else
        {
            Debug.LogError("no such id in registry");
            return null;
        }
    }

    public void RegisterUILayer(Guid id, LayerInteractionController controller)
    {
        if (UILayers.ContainsKey(id))
        {
            Debug.LogError("duplicate id in registry");
        }

        UILayers.Add(id, controller);
    }

    public void DeleteUILayer(Guid id)
    {
        GameObject uilayer = UILayers[id].ParentObj;
        UILayers.Remove(id);
        Destroy(uilayer);
    }

    public MeshData GetMeshData(Guid id)
    {
        try
        {
            return MeshDataEntries[id];
        }
        catch (Exception e)
        {
            Debug.LogError("Couldn't fetch MeshData");
            throw e;
        }
    }

    public void UpdateMeshData(MeshData meshData) => MeshDataEntries[meshData.ID] = meshData;

    public void RegisterMeshData(MeshData meshData)
    {
        if (MeshDataEntries.ContainsKey(meshData.ID))
        {
            Debug.LogError("duplicate id in registry");
        }

        MeshDataEntries.Add(meshData.ID, meshData);
    }

    public void DeleteMeshData(Guid id) => MeshDataEntries.Remove(id);
}

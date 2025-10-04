using System;
using System.Collections.Generic;
using UnityEngine;

public class MeshRegistry : MonoBehaviour
{
    private readonly Dictionary<ushort, GameObject> ArtMeshes = new();
    private readonly Dictionary<ushort, LayerInteractionController> UILayers = new();
    private readonly Dictionary<ushort, MeshData> MeshDataEntries = new();

    public static MeshRegistry instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(this);
        }
    }

    public GameObject GetArtMesh(ushort id)
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

    public void RegisterArtMeshObj(ushort id, GameObject artMeshObj)
    {
        if (ArtMeshes.ContainsKey(id))
        {
            Debug.LogError("duplicate id in registry");
        }

        ArtMeshes.Add(id, artMeshObj);
    }

    public void DeleteArtMeshObj(ushort id)
    {
        GameObject artMesh = ArtMeshes[id];
        ArtMeshes.Remove(id);
        Destroy(artMesh);
    }

    public LayerInteractionController GetUILayer(ushort id)
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

    public void RegisterUILayer(ushort id, LayerInteractionController controller)
    {
        if (UILayers.ContainsKey(id))
        {
            Debug.LogError("duplicate id in registry");
        }

        UILayers.Add(id, controller);
    }

    public void DeleteUILayer(ushort id)
    {
        GameObject uilayer = UILayers[id].ParentObj;
        UILayers.Remove(id);
        Destroy(uilayer);
    }

    public MeshData GetMeshData(ushort id)
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

    public void UpdateMeshData(MeshData meshData)
    {
        MeshDataEntries[meshData.ID] = meshData;
    }

    public void RegisterMeshData(MeshData meshData)
    {
        if (MeshDataEntries.ContainsKey(meshData.ID))
        {
            Debug.LogError("duplicate id in registry");
        }

        MeshDataEntries.Add(meshData.ID, meshData);
    }

    public void DeleteMeshData(ushort id)
    {
        MeshDataEntries.Remove(id);
    }
}

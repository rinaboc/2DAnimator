using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class LayerManager : MonoBehaviour
{
    [Header("Art Mesh creation")]
    [SerializeField] private GameObject ArtObjectPrefab;
    [SerializeField] private GameObject ViewportScale;

    [Header("UI settings")]
    [SerializeField] private GameObject UIArtLayerPrefab;
    [SerializeField] private GameObject UILayersContent;

    /// <summary>
    /// Currently selected mesh data's id
    /// </summary>
    private int _selectedLayer = -1;

    private GameObject SelectedUILayer
    {
        get
        {
            if (_selectedLayer >= 0)
            {
                return MeshRegistry.instance.GetUILayer((ushort)_selectedLayer).ParentObj;
            }
            else return null;
        }
    }

    public ArtMesh SelectedArtMesh
    {
        get
        {
            if (_selectedLayer >= 0)
            {
                return MeshRegistry.instance.GetArtMesh((ushort)_selectedLayer).GetComponent<ArtMesh>();
            }
            else return null;
        }
    }

    public static LayerManager instance;

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

    /// <summary>
    /// Create an ArtObject inside the viewport and assign the input texture as the sprite.
    /// </summary>
    public void CreateArtMesh(Texture2D texture, string path)
    {
        MeshData newMesh = new(path);
        MeshRegistry.instance.RegisterMeshData(newMesh);

        // create ArtObject inside viewport and assign the image to its sprite
        GameObject newArtObject = Instantiate(ArtObjectPrefab, ViewportScale.transform, false);
        newArtObject.name = "ArtObject" + newMesh.ID;
        newArtObject.GetComponent<ArtMesh>()
            .LoadSprite(texture)
            .SetMeshID(newMesh.ID)
            .SetSelected(false);

        MeshRegistry.instance.RegisterArtMeshObj(newMesh.ID, newArtObject);

        CreateUIArtLayer(newMesh);
        SelectUIArtLayer(newMesh.ID);
    }

    /// <summary>
    /// Creates a UI element to represent the ArtLayers in the project.
    /// </summary>
    private void CreateUIArtLayer(MeshData meshData)
    {
        GameObject newArtLayer = Instantiate(UIArtLayerPrefab, UILayersContent.transform);
        newArtLayer.name = "ArtLayer" + meshData.ID;
        newArtLayer.transform.SetSiblingIndex(0);

        newArtLayer.GetComponentInChildren<LayerInteractionController>()
            .SetID(meshData.ID)
            .SetText(meshData.name)
            .SetSelected(false);

        newArtLayer.GetComponent<Button>().onClick.AddListener(() =>
        {
            SelectUIArtLayer(meshData.ID);
        });

        MeshRegistry.instance.RegisterUILayer(meshData.ID, newArtLayer.GetComponentInChildren<LayerInteractionController>());
    }

    public void SelectUIArtLayer(int id)
    {
        // deselect previous layer
        if (SelectedUILayer != null)
        {
            SelectedUILayer.GetComponentInChildren<LayerInteractionController>().SetSelected(false);
            SelectedArtMesh.SetSelected(false);
        }

        // select the new layer
        _selectedLayer = id;
        SelectedUILayer
            .GetComponentInChildren<LayerInteractionController>()
            .SetSelected(true);
        SelectedArtMesh.SetSelected(true);

        ParameterManager.instance.HighlightCreatedCurves(SelectedArtMesh.MeshID);

    }

    public void MoveUIArtLayerUp()
    {
        if (_selectedLayer < 0) return; // nothing is selected

        Transform selectedArtLayer = SelectedUILayer.transform;
        int artLayerIndex = selectedArtLayer.GetSiblingIndex();

        if (artLayerIndex > 0)
        {
            int swappedID = UILayersContent.transform.GetChild(artLayerIndex - 1).gameObject.GetComponentInChildren<LayerInteractionController>().LayerID;
            ArtMesh swappedMesh = MeshRegistry.instance.GetArtMesh((ushort)swappedID).GetComponent<ArtMesh>();
            SelectedArtMesh.SwapDrawOrder(swappedMesh);

            selectedArtLayer.SetSiblingIndex(artLayerIndex - 1);

        }
    }

    public void MoveUIArtLayerDown()
    {
        if (_selectedLayer < 0) return;

        Transform selectedArtLayer = SelectedUILayer.transform;
        int artLayerIndex = selectedArtLayer.GetSiblingIndex();

        if (artLayerIndex < UILayersContent.transform.childCount - 1)
        {
            int swappedID = UILayersContent.transform.GetChild(artLayerIndex + 1).gameObject.GetComponentInChildren<LayerInteractionController>().LayerID;
            ArtMesh swappedMesh = MeshRegistry.instance.GetArtMesh((ushort)swappedID).GetComponent<ArtMesh>();
            SelectedArtMesh.SwapDrawOrder(swappedMesh);

            selectedArtLayer.SetSiblingIndex(artLayerIndex + 1);
        }
    }

    public void DeleteSelectedArtObject()
    {
        if (_selectedLayer < 0) return;

        ParameterManager.instance.DeleteParamPointsOfMesh(SelectedArtMesh.MeshID);

        MeshRegistry.instance.DeleteArtMeshObj((ushort)_selectedLayer);
        MeshRegistry.instance.DeleteUILayer((ushort)_selectedLayer);
        MeshRegistry.instance.DeleteMeshData((ushort)_selectedLayer);

        // deselect
        _selectedLayer = -1;
    }
}

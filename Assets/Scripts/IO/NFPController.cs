using UnityEngine;
using System.IO;
using System;
using Assets.Scripts.Utility.MVI;
using Assets.Scripts.States;

public class NFPController : MonoBehaviour, IView<OperationState>
{
    private IViewModel<OperationState> _viewModel;

    void Start()
    {
        RequestPermissionAsynchronously(false);
    }

    private async void RequestPermissionAsynchronously(bool readPermissionOnly = false)
    {
        NativeFilePicker.Permission permission = await NativeFilePicker.RequestPermissionAsync(readPermissionOnly);
        Debug.Log("Permission result: " + permission);
    }

    public void OpenImageFile()
    {
        // setting platform specific filetypes for images
#if UNITY_ANDROID
        string[] fileTypes = new string[] { "image/*" };
#elif UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX
        string[] fileTypes = new string[] { "png", "jpeg", "jpg" };
#else
        string[] fileTypes = new string[] { "public.image" };
#endif

        // Pick an image file
        NativeFilePicker.PickFile((path) =>
        {
            if (LoadImage(path, out Texture2D texture))
            {
                // create new artmesh
                _viewModel.Send(new CreateMeshLayerIntent(Guid.NewGuid(), texture, path));

                // MeshData meshData = MeshManager.Instance.CreateArtMeshObj(texture, path);
                // LayerManager.Instance.CreateUIArtLayer(meshData);
            }

        }, fileTypes);
    }

    public static bool LoadImage(string path, out Texture2D texture)
    {
        texture = new Texture2D(1, 1, textureFormat: TextureFormat.RGBA32, false);
        try
        {
            // load image
            byte[] bytes = File.ReadAllBytes(path);
            texture.filterMode = FilterMode.Trilinear;
            texture.LoadImage(bytes);

            Debug.Log("Picked file: " + path);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError("Couldn't load image.");
            Debug.LogError(e.StackTrace);
        }

        return false;
    }

    public void Render(OperationState state)
    {
    }

    public void SetViewModel(IViewModel<OperationState> viewModel)
    {
        _viewModel = viewModel;
        _viewModel.Bind(this);
    }
}

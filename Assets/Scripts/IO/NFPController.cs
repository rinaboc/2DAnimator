using UnityEngine;
using System.IO;
using System;
using Assets.Scripts.Utility.MVI;
using Assets.Scripts.States;
using SFB;

public class NFPController : MonoBehaviour
{
    [SerializeField] private AppInitializer _appInitializer;
    private IViewModel<OperationState> _viewModel;

    void Start()
    {
        RequestPermissionAsynchronously(false);
        if (!_appInitializer.GetViewModel(out _viewModel))
        {
            Debug.LogError("Couldn't fetch viewModel");
        }
    }

    private async void RequestPermissionAsynchronously(bool readPermissionOnly = false)
    {
        NativeFilePicker.Permission permission = await NativeFilePicker.RequestPermissionAsync(readPermissionOnly);
        Debug.Log("Permission result: " + permission);
    }

    public void OpenProject()
    {
        string[] fileTypes = new string[] { "tda" };

        NativeFilePicker.PickFile((path) =>
        {
            _viewModel?.Send(new OpenProjectIntent(path));
        }, fileTypes);
    }

    public void SaveProject()
    {
#if UNITY_ANDROID || UNITY_IOS
    NativeFilePicker.ExportFile((path) =>
    {
        if (!string.IsNullOrEmpty(path))
            _viewModel?.Send(new SaveProjectIntent(path));
    }, "tda", "MyProject.tda");
#elif UNITY_STANDALONE_WIN
        var extensionList = new[] { new ExtensionFilter("2DAnimator project", "tda") };
        StandaloneFileBrowser.SaveFilePanelAsync("Save Project", "", "project", extensionList, (path) =>
        {
            if (path != null)
                _viewModel?.Send(new SaveProjectIntent(path));
        });
#endif

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
                _viewModel?.Send(new CreateMeshLayerIntent(Guid.NewGuid(), texture, path));
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
}

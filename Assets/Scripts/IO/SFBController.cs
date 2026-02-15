using UnityEngine;
using System.IO;
using System;
using Assets.Scripts.Utility.MVI;
using Assets.Scripts.States;
using SimpleFileBrowser;

public class SFBController : MonoBehaviour
{
    [SerializeField] private AppInitializer _appInitializer;
    private IViewModel<MeshLayerStates, LayerStates> _viewModel;

    void Start()
    {
        FileBrowser.SetFilters(false);
        FileBrowser.SetDefaultFilter(".tda");
        RequestPermissionAsynchronously();

        if (!_appInitializer.GetViewModel(out _viewModel))
        {
            Debug.LogError("Couldn't fetch viewModel");
        }
    }

    private void RequestPermissionAsynchronously()
    {
#if UNITY_ANDROID || UNITY_IOS
        // if (!FileBrowser.CheckPermission())
        FileBrowser.RequestPermissionAsync((permission) =>
        {
            if (!(permission == FileBrowser.Permission.Granted))
                Debug.LogError("No permission received to use file browser");
        });
#endif
    }

    public void OpenProject()
    {
        FileBrowser.SetFilters(false, new FileBrowser.Filter("2DAnimator Project", ".tda"));
        FileBrowser.SetDefaultFilter(".tda");

        FileBrowser.ShowLoadDialog(
            onSuccess: (paths) =>
            {
                if (paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
                {
                    _viewModel?.Send(new OpenProjectIntent(paths[0]));
                }
            },
            onCancel: () => { },
            pickMode: FileBrowser.PickMode.Files,
            allowMultiSelection: false,
            title: "Open Project"
        );
    }

    public void SaveProject()
    {
        FileBrowser.SetFilters(false, new FileBrowser.Filter("2DAnimator Project", ".tda"));
        FileBrowser.SetDefaultFilter(".tda");

        FileBrowser.ShowSaveDialog(
            onSuccess: (paths) =>
            {
                if (paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
                {
                    _viewModel?.Send(new SaveProjectIntent(paths[0]));
                }
            },
            onCancel: () => { },
            pickMode: FileBrowser.PickMode.Files,
            title: "Save Project",
            initialFilename: "project.tda"
        );
    }

    public void OpenImageFile()
    {
        FileBrowser.SetFilters(false,
            new FileBrowser.Filter("Images", ".png", ".jpg", ".jpeg")
        );
        FileBrowser.SetDefaultFilter(".png");

        FileBrowser.ShowLoadDialog(
            onSuccess: (paths) =>
            {
                if (paths.Length == 0 || string.IsNullOrEmpty(paths[0]))
                    return;

                foreach (string path in paths)
                    if (LoadImage(path, out Texture2D texture))
                        _viewModel?.Send(new CreateMeshLayerIntent(Guid.NewGuid(), texture, path));

            },
            onCancel: () => { },
            pickMode: FileBrowser.PickMode.Files,
            allowMultiSelection: true,
            title: "Open Image"
        );
    }

    public static bool LoadImage(string path, out Texture2D texture)
    {
        texture = new Texture2D(1, 1, textureFormat: TextureFormat.RGBA32, false);
        try
        {
            // load image
            byte[] bytes = FileBrowserHelpers.ReadBytesFromFile(path);
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

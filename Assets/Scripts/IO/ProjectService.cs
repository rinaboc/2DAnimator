using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using Assets.Scripts.Utility.MVI;
using SimpleFileBrowser;
using UnityEngine;

public interface IProjectService
{
    SaveData Load(string path);
    void Save(string path, IModelContext context);
}

public class ProjectService : IProjectService
{
    public void Save(string path, IModelContext context)
    {
        var saveData = CreateSaveData(context);
        Debug.Log(path);

        // serialize savedata
        BinaryFormatter binaryFormatter = new();
        using MemoryStream ms = new();
        binaryFormatter.Serialize(ms, saveData);
        byte[] data = ms.ToArray();
        FileBrowserHelpers.WriteBytesToFile(path, data);
    }

    private SaveData CreateSaveData(IModelContext context)
    {
        return new()
        {
            MeshDatas = context.Meshes.GetAll().ToArray(),
            Parameters = context.Parameters.GetAll().ToArray(),
            ParamCurves = context.ParamCurves.GetAll().ToArray(),
            ParamPoints = context.ParamPoints.GetAll().ToArray(),
            KeyFrames = context.KeyFrames.GetAll().ToArray(),
            AnimationSetting = new(context.GeneralSettings.MaxFrames, context.GeneralSettings.FramePerSec)
        };
    }

    public SaveData Load(string path)
    {
        if (!FileBrowserHelpers.FileExists(path))
            throw new FileNotFoundException($"Project file not found: {path}");

        byte[] data = FileBrowserHelpers.ReadBytesFromFile(path);
        BinaryFormatter binaryFormatter = new();
        using MemoryStream ms = new(data);
        SaveData save = (SaveData)binaryFormatter.Deserialize(ms);
        return save;
    }
}

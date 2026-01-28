using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using Assets.Scripts.Utility.MVI;
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
        FileStream fs = File.Create(path);
        BinaryFormatter binaryFormatter = new();
        binaryFormatter.Serialize(fs, saveData);
        fs.Close();
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
        if (!File.Exists(path))
            throw new FileNotFoundException($"Project file not found: {path}");

        FileStream fs = File.Open(path, FileMode.Open);
        BinaryFormatter binaryFormatter = new();
        SaveData save = (SaveData)binaryFormatter.Deserialize(fs);
        fs.Close();

        return save;
    }
}

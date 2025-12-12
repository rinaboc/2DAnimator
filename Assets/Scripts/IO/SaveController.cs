using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class SaveController : MonoBehaviour
{
    private static readonly List<ISaveable> saveables = new();
    private static readonly List<ILoadable> loadables = new();
    private string savePath;

    private void Awake()
    {
        savePath = Application.persistentDataPath + "/program.save";
    }

    public static void Register(ISaveable saveable)
    {
        saveables.Add(saveable);
    }

    public static void Register(ILoadable loadable)
    {
        loadables.Add(loadable);
    }

    public void Save()
    {
        SaveData saveData = new();
        foreach (var item in saveables)
        {
            item.SaveState(saveData);
        }

        Debug.Log(savePath);

        // serialize savedata
        FileStream fs = File.Create(savePath);
        BinaryFormatter binaryFormatter = new();
        binaryFormatter.Serialize(fs, saveData);
        fs.Close();
    }

    public void Load()
    {
        if (!File.Exists(savePath)) return;

        FileStream fs = File.Open(savePath, FileMode.Open);
        BinaryFormatter binaryFormatter = new();
        SaveData save = (SaveData)binaryFormatter.Deserialize(fs);
        fs.Close();

        foreach (var item in loadables)
        {
            item.LoadState(save);
        }
    }
}

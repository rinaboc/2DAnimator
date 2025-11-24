using UnityEngine;

public interface ISaveable
{
    void SaveState(ref SaveData saveData);
    void RegisterSaveable();
}

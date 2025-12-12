using UnityEngine;

public interface ISaveable
{
    void SaveState(SaveData saveData);
    void RegisterSaveable();
}

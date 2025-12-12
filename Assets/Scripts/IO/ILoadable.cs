using UnityEngine;

public interface ILoadable
{
    void LoadState(SaveData saveData);
    void RegisterLoadable();
}

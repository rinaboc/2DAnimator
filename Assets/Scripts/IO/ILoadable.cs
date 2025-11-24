using UnityEngine;

public interface ILoadable
{
    void LoadState(ref SaveData saveData);
    void RegisterLoadable();
}

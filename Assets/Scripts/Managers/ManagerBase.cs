using UnityEngine;

public abstract class ManagerBase<T> : MonoBehaviour, ILoadable where T : MonoBehaviour
{
    public static T Instance { get; private set; }

    protected virtual void Awake()
    {
        if (Instance == null)
        {
            Instance = this as T;
            RegisterLoadable();
        }
        else
        {
            Destroy(this);
        }
    }

    public abstract void LoadState(SaveData saveData);

    public void RegisterLoadable()
    {
        SaveController.Register(this);
    }
}

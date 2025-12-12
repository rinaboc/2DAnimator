using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base class for registries that store entries of type T
/// </summary>
/// <typeparam name="T">Must be a subclass of EntityBase</typeparam>
/// <typeparam name="L">Is the actual type of the registry object
public abstract class RegistryBase<T, L> : ScriptableObject, ISaveable
    where T : EntityBase
    where L : RegistryBase<T, L>
{
    protected static L _instance;

    public static L Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<L>(typeof(L).Name);
                _instance.RegisterSaveable();

                if (_instance == null)
                {
                    Debug.LogError($"{typeof(L).Name} asset not found in Resources!");
                }
            }

            return _instance;
        }
    }

    /// <summary>
    /// Dictionary to store the registered entries of type T
    /// </summary>
    protected Dictionary<Guid, T> _map = new();

    public virtual bool TryGet(Guid id, out T value) => _map.TryGetValue(id, out value);
    public virtual bool Register(T value) => _map.TryAdd(value.ID, value);
    public virtual bool Remove(Guid id) => _map.Remove(id);

    public IReadOnlyCollection<T> GetAll() => _map.Values;

    public void Clear() => _map.Clear();

    /// <summary>
    /// Get multiple entries by a list of IDs
    /// </summary>
    public List<T> GetEntries(List<Guid> ids)
    {
        List<T> retEntries = new();

        for (int i = 0; i < ids.Count; i++)
        {
            if (TryGet(ids[i], out T entry))
                retEntries.Add(entry);
        }

        return retEntries;
    }

    public abstract void SaveState(SaveData saveData);

    public void RegisterSaveable()
    {
        SaveController.Register(this);
    }
}

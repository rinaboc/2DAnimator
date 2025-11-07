// RegistryBase.cs (ScriptableObject)
using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class RegistryBase<T> : ScriptableObject where T : EntityBase
{
    protected Dictionary<Guid, T> _map = new();

    public virtual bool TryGet(Guid id, out T value) => _map.TryGetValue(id, out value);

    public virtual bool Register(T value) => _map.TryAdd(value.ID, value);

    public virtual bool Remove(Guid id) => _map.Remove(id);

    public IReadOnlyCollection<T> GetAll() => _map.Values;

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
}

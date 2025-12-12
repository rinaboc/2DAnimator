using System;
using System.Runtime.Serialization;

[Serializable]
public abstract class EntityBase
{
    public Guid ID;

    protected EntityBase(bool autoRegister = true)
    {
        ID = Guid.NewGuid();

        if (autoRegister) Register();
    }

    protected abstract void Register();
}

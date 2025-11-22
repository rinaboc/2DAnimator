using System;

[Serializable]
public abstract class EntityBase
{
    public readonly Guid ID;

    protected EntityBase(bool autoRegister = true)
    {
        ID = Guid.NewGuid();

        if (autoRegister) Register();
    }

    protected abstract void Register();
}

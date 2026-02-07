using System;

[Serializable]
public abstract class EntityBase
{
    public Guid ID;

    public EntityBase()
    {
        ID = Guid.NewGuid();
    }
}

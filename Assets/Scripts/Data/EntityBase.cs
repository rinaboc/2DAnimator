using System;

[Serializable]
public abstract class EntityBase
{
    public Guid ID;

    protected EntityBase()
    {
        ID = new Guid();
    }
}

using System.Collections.Generic;

public static class SaveController
{
    private static readonly List<ISaveable> saveables = new();
    private static readonly List<ILoadable> loadables = new();

    public static void Register(ISaveable saveable)
    {
        saveables.Add(saveable);
    }

    public static void Register(ILoadable loadable)
    {
        loadables.Add(loadable);
    }

    public static void Save()
    {

    }

    public static void Load()
    {

    }
}

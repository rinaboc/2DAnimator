using UnityEngine;

[CreateAssetMenu(fileName = "GeneralSettings", menuName = "Scriptable Objects/GeneralSettings")]
public class GeneralSettings : ScriptableObject, ISaveable
{
    public int MaxFrames = 24;
    public int FramePerSec = 16;

    protected static GeneralSettings _instance;

    public static GeneralSettings Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<GeneralSettings>("GeneralSettings");
                _instance.RegisterSaveable();

                if (_instance == null)
                {
                    Debug.LogError("GeneralSettings asset not found in Resources!");
                }
            }

            return _instance;
        }
    }

    public void RegisterSaveable()
    {
        SaveController.Register(this);
    }

    public void SaveState(SaveData saveData)
    {
        AnimationSettings animation = new(MaxFrames, FramePerSec);
        saveData.AnimationSetting = animation;
    }
}

using System;
using UnityEngine;

public interface IGeneralSettings
{
    static GeneralSettings Instance { get; }
    int MaxFrames { get; set; }
    int FramePerSec { get; set; }
    Guid SelectedParamID { get; set; }
}

[CreateAssetMenu(fileName = "GeneralSettings", menuName = "Scriptable Objects/GeneralSettings")]
public class GeneralSettings : ScriptableObject, ISaveable, IGeneralSettings
{
    public int MaxFrames { get; set; }
    public int FramePerSec { get; set; }
    public Guid SelectedParamID { get; set; }

    protected static GeneralSettings _instance;

    public static GeneralSettings Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<GeneralSettings>("GeneralSettings");
                _instance.RegisterSaveable();
                _instance.MaxFrames = 24;
                _instance.FramePerSec = 16;
                _instance.SelectedParamID = Guid.Empty;

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

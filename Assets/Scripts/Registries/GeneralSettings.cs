using System;
using UnityEngine;

public interface IGeneralSettings
{
    static GeneralSettings Instance { get; }
    int MaxFrames { get; set; }
    int FramePerSec { get; set; }
    Guid SelectedParamID { get; set; }
    Guid SelectedMeshID { get; set; }
    int CurrentFrame { get; set; }
    int HistoryLimit { get; set; }
}

[CreateAssetMenu(fileName = "GeneralSettings", menuName = "Scriptable Objects/GeneralSettings")]
public class GeneralSettings : ScriptableObject, IGeneralSettings
{
    public int MaxFrames { get; set; }
    public int FramePerSec { get; set; }
    public Guid SelectedParamID { get; set; }
    public Guid SelectedMeshID { get; set; }
    public int CurrentFrame { get; set; }
    public int HistoryLimit { get; set; }

    protected static GeneralSettings _instance;

    public static GeneralSettings Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<GeneralSettings>("GeneralSettings");
                _instance.MaxFrames = 24;
                _instance.FramePerSec = 16;
                _instance.SelectedParamID = Guid.Empty;
                _instance.SelectedMeshID = Guid.Empty;
                _instance.CurrentFrame = 1;
                _instance.HistoryLimit = 40;

                if (_instance == null)
                {
                    Debug.LogError("GeneralSettings asset not found in Resources!");
                }
            }

            return _instance;
        }
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "KeyFrameRegistry", menuName = "Global/KeyFrame Registry")]
public class KeyFrameRegistry : ScriptableObject
{

    [SerializeField] private Dictionary<Guid, KeyFrame> _keyframes = new();

    public Dictionary<Guid, KeyFrame> GetAllKeyFrames => _keyframes;

    private static KeyFrameRegistry _instance;
    public static KeyFrameRegistry Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<KeyFrameRegistry>("KeyFrameRegistry");

                if (_instance == null)
                {
                    Debug.LogError("KeyFrameRegistry asset not found in Resources!");
                }
            }

            return _instance;
        }
    }

    public bool AddKeyframe(KeyFrame keyframe)
    {
        if (_keyframes.ContainsKey(keyframe.ID))
        {
            return false;
        }

        foreach ((_, KeyFrame item) in _keyframes)
        {
            if (item.IsSameCell(keyframe)) // should update
            {
                item.ParamValue = keyframe.ParamValue;
                return true;
            }
        }

        _keyframes.Add(keyframe.ID, keyframe);
        return true;
    }

    public bool RemoveKeyframe(Guid id) => _keyframes.Remove(id);

    public KeyFrame GetKeyFrame(Guid id) => _keyframes[id];
}

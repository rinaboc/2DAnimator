using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "KeyFrameRegistry", menuName = "Global/KeyFrame Registry")]
public class KeyFrameRegistry : ScriptableObject
{
    [SerializeField] private Dictionary<Guid, KeyFrame> _keyframes = new();

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

        KeyFrame duplicateKey = null;
        foreach ((_, KeyFrame item) in _keyframes)
        {
            if (item.IsSameCell(keyframe)) // should update
            {
                duplicateKey = item;
                break;
            }
        }

        if (duplicateKey != null)
        {
            RemoveKeyframe(duplicateKey.ID);
        }

        _keyframes.Add(keyframe.ID, keyframe);
        return true;
    }

    public bool RemoveKeyframe(Guid id) => _keyframes.Remove(id);

    public bool GetKeyFrame(Guid id, out KeyFrame keyFrame) => _keyframes.TryGetValue(id, out keyFrame);

    public List<KeyFrame> GetKeyFramesOfParam(Guid paramID)
    {
        List<KeyFrame> ret = new();

        foreach (var item in _keyframes)
        {
            if (item.Value.ParamID.Equals(paramID))
            {
                ret.Add(item.Value);
            }
        }

        if (ret.Count > 1)
        {
            ret.Sort((x, y) => x.Frame.CompareTo(y.Frame));
        }
        return ret;
    }
}

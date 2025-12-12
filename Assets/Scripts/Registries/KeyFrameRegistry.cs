using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "KeyFrameRegistry", menuName = "Global/KeyFrameRegistry")]
public class KeyFrameRegistry : RegistryBase<KeyFrame, KeyFrameRegistry>
{
    public override bool Register(KeyFrame keyframe)
    {
        if (_map.ContainsKey(keyframe.ID))
        {
            return false;
        }

        KeyFrame duplicateKey = null;
        foreach ((_, KeyFrame item) in _map)
        {
            if (item.IsSameCell(keyframe)) // should update
            {
                duplicateKey = item;
                break;
            }
        }

        if (duplicateKey != null)
        {
            Remove(duplicateKey.ID);
        }

        _map.Add(keyframe.ID, keyframe);
        return true;
    }

    public List<KeyFrame> GetKeyFramesOfParam(Guid paramID)
    {
        List<KeyFrame> ret = new();

        foreach (var item in _map)
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

    public override void SaveState(SaveData saveData)
    {
        saveData.KeyFrames = new KeyFrame[_map.Count];
        saveData.KeyFrames = _map.Values.ToArray();
    }
}

using System;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "ParamPointRegistry", menuName = "Global/ParamPointRegistry")]
public class ParamPointRegistry : RegistryBase<ParamPoint, ParamPointRegistry>
{
    public override void SaveState(SaveData saveData)
    {
        saveData.ParamPoints = new ParamPoint[_map.Count];
        saveData.ParamPoints = _map.Values.ToArray();
    }
}

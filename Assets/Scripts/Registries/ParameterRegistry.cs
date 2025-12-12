using System;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "ParameterRegistry", menuName = "Global/ParameterRegistry")]
public class ParameterRegistry : RegistryBase<Parameter, ParameterRegistry>
{
    public override void SaveState(SaveData saveData)
    {
        saveData.Parameters = new Parameter[_map.Count];
        saveData.Parameters = _map.Values.ToArray();
    }
}

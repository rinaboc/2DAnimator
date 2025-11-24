using System;
using UnityEngine;

[CreateAssetMenu(fileName = "ParameterRegistry", menuName = "Global/ParameterRegistry")]
public class ParameterRegistry : RegistryBase<Parameter, ParameterRegistry>
{
    public override void SaveState(ref SaveData saveData)
    {
        throw new NotImplementedException();
    }
}

using System;
using UnityEngine;

[CreateAssetMenu(fileName = "ParamPointRegistry", menuName = "Global/ParamPointRegistry")]
public class ParamPointRegistry : RegistryBase<ParamPoint, ParamPointRegistry>
{
    public override void SaveState(ref SaveData saveData)
    {
        throw new NotImplementedException();
    }
}

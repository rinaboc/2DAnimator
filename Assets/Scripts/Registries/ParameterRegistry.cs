using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "ParameterRegistry", menuName = "Global/Parameter Registry")]
public class ParameterRegistry : RegistryBase<Parameter>
{
    private static ParameterRegistry _instance;
    public static ParameterRegistry Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<ParameterRegistry>("ParameterRegistry");

                if (_instance == null)
                {
                    Debug.LogError("ParameterRegistry asset not found in Resources!");
                }
            }

            return _instance;
        }
    }

    public override void SaveState(ref SaveData saveData)
    {
        throw new NotImplementedException();
    }
}

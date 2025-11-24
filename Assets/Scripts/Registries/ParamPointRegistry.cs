using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ParamPointRegistry", menuName = "Global/ParamPointRegistry")]
public class ParamPointRegistry : RegistryBase<ParamPoint>
{
    private static ParamPointRegistry _instance;
    public static ParamPointRegistry Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<ParamPointRegistry>("ParamPointRegistry");

                if (_instance == null)
                {
                    Debug.LogError("ParamPointRegistry asset not found in Resources!");
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

using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "MeshRegistry", menuName = "Global/MeshRegistry")]
public class MeshRegistry : RegistryBase<MeshData, MeshRegistry>
{
    public override void SaveState(SaveData saveData)
    {
        saveData.MeshDatas = new MeshData[_map.Count];
        saveData.MeshDatas = _map.Values.ToArray();
    }
}

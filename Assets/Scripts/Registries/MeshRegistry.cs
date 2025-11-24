using UnityEngine;

[CreateAssetMenu(fileName = "MeshRegistry", menuName = "Global/MeshRegistry")]
public class MeshRegistry : RegistryBase<MeshData, MeshRegistry>
{
    public override void SaveState(ref SaveData saveData)
    {
        throw new System.NotImplementedException();
    }
}

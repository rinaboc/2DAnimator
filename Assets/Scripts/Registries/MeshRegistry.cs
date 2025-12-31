using System;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "MeshRegistry", menuName = "Global/MeshRegistry")]
public class MeshRegistry : RegistryBase<MeshData, MeshRegistry>
{
    public bool TryGetPreviousDrawOrder(ushort drawOrder, out MeshData previous)
    {
        previous = null;
        Guid retID = Guid.Empty;
        ushort inf = 0;
        foreach ((_, MeshData meshData) in _map)
        {
            if (meshData.drawOrder < drawOrder && meshData.drawOrder > inf)
            {
                inf = meshData.drawOrder;
                retID = meshData.ID;
            }
        }

        if (retID == Guid.Empty)
            return false;

        return TryGet(retID, out previous);
    }

    public bool TryGetNextDrawOrder(ushort drawOrder, out MeshData next)
    {
        next = null;
        Guid retID = Guid.Empty;
        ushort inf = ushort.MaxValue;
        foreach ((_, MeshData meshData) in _map)
        {
            if (meshData.drawOrder > drawOrder && meshData.drawOrder < inf)
            {
                inf = meshData.drawOrder;
                retID = meshData.ID;
            }
        }

        if (retID == Guid.Empty)
            return false;

        return TryGet(retID, out next);
    }

    public override void SaveState(SaveData saveData)
    {
        saveData.MeshDatas = new MeshData[_map.Count];
        saveData.MeshDatas = _map.Values.ToArray();
    }
}

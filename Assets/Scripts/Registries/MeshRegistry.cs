using UnityEngine;

[CreateAssetMenu(fileName = "MeshRegistry", menuName = "Global/Mesh Registry")]
public class MeshRegistry : RegistryBase<MeshData>
{
    private static MeshRegistry _instance;
    public static MeshRegistry Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<MeshRegistry>("MeshRegistry");

                if (_instance == null)
                {
                    Debug.LogError("MeshRegistry asset not found in Resources!");
                }
            }

            return _instance;
        }
    }
}

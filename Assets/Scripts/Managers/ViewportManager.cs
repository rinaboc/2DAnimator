using UnityEngine;

public class ViewportManager : ManagerBase<ViewportManager>
{
    [SerializeField] private Transform _topLeftAnchor;
    public Vector3 TopLeftAnchor
    {
        get
        {
            return _topLeftAnchor.position;
        }
    }
    [SerializeField] private Transform _bottomRightAnchor;
    public Vector3 BottomRightAnchor
    {
        get
        {
            return _bottomRightAnchor.position;
        }
    }

    public override void LoadState(SaveData saveData)
    {

    }
}

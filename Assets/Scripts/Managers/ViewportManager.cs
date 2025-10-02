using UnityEngine;

public class ViewportManager : MonoBehaviour
{
    public static ViewportManager instance;

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

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(this);
        }
    }

}

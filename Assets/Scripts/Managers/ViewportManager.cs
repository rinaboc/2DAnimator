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

    [SerializeField] private RectTransform _viewport;

    [SerializeField] private GameObject _sideBar;

    void Start()
    {
        Canvas.ForceUpdateCanvases();

        if (_viewport != null)
        {
            var size = _viewport.sizeDelta;

            if (_viewport.TryGetComponent<BoxCollider>(out var boxCollider))
            {
                boxCollider.size = new Vector3(size.x, size.y, 0.1f);
                boxCollider.center = new Vector3(-size.x * 0.5f, 0f, 0f);
            }
        }
    }

    public void SetSidebarVisibility(bool visible)
    {
        _sideBar.SetActive(visible);

        Canvas.ForceUpdateCanvases();
        if (_viewport != null)
        {
            var size = _viewport.sizeDelta;

            if (_viewport.TryGetComponent<BoxCollider>(out var boxCollider))
            {
                boxCollider.size = new Vector3(size.x, size.y, 0.1f);
                boxCollider.center = new Vector3(-size.x * 0.5f, 0f, 0f);
            }
        }
    }
}

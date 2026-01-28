using UnityEngine;

public class ScaleManager : ManagerBase<ScaleManager>
{
    [SerializeField] private RectTransform ViewportScaledTransform;
    public static readonly float MaxScale = 800f;
    public static readonly float MinScale = 50f;
    public static readonly float ScaleSpeed = 0.15f;

    public static readonly float OriginalScale = 100f;
    private float _currentScale;
    public float CurrentScale
    {
        get { return _currentScale; }
        set
        {
            if (value < MaxScale && value > MinScale)
            {
                _currentScale = value;
                ViewportScaledTransform.localScale = Vector3.one * value;
                ViewportEvents.ScaleChangeEvent.Invoke();
            }
        }
    }

    void OnEnable()
    {
        _currentScale = OriginalScale;
        CurrentScale = OriginalScale;
    }
}

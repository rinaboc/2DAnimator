using UnityEngine;
using UnityEngine.Events;

public class ScaleManager : MonoBehaviour
{
    [SerializeField] private RectTransform ViewportScaledTransform;
    public UnityEvent onScaleChange = new();

    public float MaxScale = 800f;
    public float MinScale = 50f;
    public float ScaleSpeed = 0.15f;

    public float OriginalScale = 100f;
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
                onScaleChange.Invoke();
            }
        }
    }


    void Start()
    {
        _currentScale = OriginalScale;
        CurrentScale = OriginalScale;
    }

    public static ScaleManager instance;

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

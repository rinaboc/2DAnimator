using UnityEngine;
using UnityEngine.UIElements;

public class TimelineWidget : MonoBehaviour
{
    private VisualElement ui;
    [SerializeField] private VisualTreeAsset framebarTemplate;
    private int _maxFrames = 20;
    private SliderInt frameSlider;
    private VisualElement framebarContainer;
    void Awake()
    {
        ui = GetComponent<UIDocument>().rootVisualElement;
        ui.dataSource = this;

        frameSlider = ui.Q<SliderInt>("frame_slider");
        framebarContainer = ui.Q<VisualElement>("framebar_container");
    }

    void Start()
    {
        frameSlider.RegisterCallback<GeometryChangedEvent>(evt =>
        {
            float width = frameSlider.resolvedStyle.width;
            float spacing = width / (_maxFrames - 1);
            framebarContainer.style.paddingLeft = -spacing / 2f;
            for (int i = 0; i < _maxFrames; i++)
            {
                VisualElement frameBar = framebarTemplate.Instantiate();
                frameBar.style.maxWidth = spacing;
                frameBar.style.width = spacing;
                frameBar.style.minWidth = spacing;
                Debug.Log(spacing);
                framebarContainer.Add(frameBar);
            }

        });
    }



}

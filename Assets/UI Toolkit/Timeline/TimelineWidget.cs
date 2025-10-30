using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

public class TimelineWidget : MonoBehaviour
{
    public static UnityEvent<Guid, float> KeyParamSliderChanged = new();
    private VisualElement ui;

    private VisualElement m_TimelineDrawer;
    private VisualElement m_Timeline;
    private TimelineSlider m_TimelineSlider;
    private Button m_OpenButton;

    private bool m_widgetOpen = false;

    void Awake()
    {
        ui = GetComponent<UIDocument>().rootVisualElement;
        ui.dataSource = this;

        m_TimelineDrawer = ui.Q<VisualElement>("TimelineDrawer");
        m_Timeline = ui.Q<VisualElement>("Timeline");
        m_OpenButton = ui.Q<Button>("TimelineOpenButton");

        m_TimelineSlider = ui.Q<TimelineSlider>("TimelineSlider");
    }

    void Start()
    {
        m_OpenButton.RegisterCallback<ClickEvent>(OnButtonClick);
        m_TimelineDrawer.AddToClassList("close-timeline");
        m_TimelineDrawer.RemoveFromClassList("open-timeline");

        KeyParamSliderChanged.AddListener(OnKeySliderChanged);
    }

    private void OnKeySliderChanged(Guid id, float value)
    {
        AnimationManager.instance.CreateKeyframe(id, value);
        m_TimelineSlider.CreateKeyframeAtCurrentFrame(id);
    }

    private void OnButtonClick(ClickEvent evt)
    {
        if (m_widgetOpen)
        {
            m_TimelineDrawer.AddToClassList("close-timeline");
            m_TimelineDrawer.RemoveFromClassList("open-timeline");
        }
        else
        {
            m_TimelineDrawer.RemoveFromClassList("close-timeline");
            m_TimelineDrawer.AddToClassList("open-timeline");
        }

        m_widgetOpen = !m_widgetOpen;

        m_Timeline.style.display = m_widgetOpen ? DisplayStyle.Flex : DisplayStyle.None;

        ParameterManager.instance.ParameterWidgetVisibility = !m_widgetOpen;
        if (m_widgetOpen)
        {
            SendParametersToTimeline();
        }
    }

    private void SendParametersToTimeline()
    {
        List<Parameter> parameters = ParameterRegistry.Instance.GetAllParameters;
        m_TimelineSlider.LoadParameters(parameters);
    }

    public int Currentframe => m_TimelineSlider.CurrentFrame;
}

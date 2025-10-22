using System;
using UnityEngine;
using UnityEngine.UIElements;

public class TimelineWidget : MonoBehaviour
{
    private VisualElement ui;

    private VisualElement m_TimelineDrawer;
    private VisualElement m_Timeline;
    private Button m_OpenButton;

    private bool m_widgetOpen = false;

    void Awake()
    {
        ui = GetComponent<UIDocument>().rootVisualElement;
        ui.dataSource = this;

        m_TimelineDrawer = ui.Q<VisualElement>("TimelineDrawer");
        m_Timeline = ui.Q<VisualElement>("Timeline");
        m_OpenButton = ui.Q<Button>("TimelineOpenButton");
    }

    void Start()
    {
        m_OpenButton.RegisterCallback<ClickEvent>(OnButtonClick);
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
    }
}

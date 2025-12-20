using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

public class TimelineSettingsController : DialogController<TimelineSettingsElement>
{
    [SerializeField] TimelineWidgetController m_TimelineWidgetController;

    [SerializeField, CreateProperty] private int m_maxFrames;
    [SerializeField, CreateProperty] private int m_framePerSec;

    void Start()
    {
        var maxFramesField = ui.Q<IntegerField>("MaxFrames");
        maxFramesField.SetBinding("value", new DataBinding
        {
            dataSource = this,
            dataSourcePath = new PropertyPath(nameof(m_maxFrames)),
            bindingMode = BindingMode.TwoWay
        });
        maxFramesField.RegisterCallback<FocusOutEvent>(evt =>
        {
            m_TimelineWidgetController.SetMaxFrames(m_maxFrames);
        });

        var framePerSecField = ui.Q<IntegerField>("FramePerSec");
        framePerSecField.SetBinding("value", new DataBinding
        {
            dataSource = this,
            dataSourcePath = new PropertyPath(nameof(m_framePerSec)),
            bindingMode = BindingMode.TwoWay
        });
        framePerSecField.RegisterCallback<FocusOutEvent>(evt =>
        {
            m_TimelineWidgetController.SetFramePerSec(m_framePerSec);
        });
    }
}

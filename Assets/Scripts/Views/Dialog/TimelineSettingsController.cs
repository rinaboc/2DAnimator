using Assets.Scripts.States;
using Assets.Scripts.Utility.MVI;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

public class TimelineSettingsController : DialogController<TimelineSettingsElement>, ILoadable, IView<TimelineState>
{
    [SerializeField] TimelineWidgetController m_TimelineWidgetController;

    [SerializeField, CreateProperty] private int m_maxFrames;
    [SerializeField, CreateProperty] private int m_framePerSec;

    private IViewModel<TimelineState> _viewModel;

    public void LoadState(SaveData saveData)
    {
        m_maxFrames = GeneralSettings.Instance.MaxFrames;
        m_framePerSec = GeneralSettings.Instance.FramePerSec;
    }

    public void RegisterLoadable()
    {
        SaveController.Register(this);
    }

    public void Render(TimelineState state)
    {
        ShowPanel(state.IsSettingsOpen);

        if (state.IsSettingsOpen)
        {
            m_maxFrames = state.MaxFrames;
            m_framePerSec = state.FramePerSec;
        }
    }

    public void SetViewModel(IViewModel<TimelineState> viewModel)
    {
        _viewModel = viewModel;
        _viewModel?.Bind(this);
    }

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
            if (m_maxFrames > 60) m_maxFrames = 60;
            if (m_maxFrames < 2) m_maxFrames = 2;

            _viewModel?.Send(new UpdateMaxFramesIntent(m_maxFrames));
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
            _viewModel?.Send(new UpdateFramePerSecIntent(m_framePerSec));
        });

        dialogElement.Q<Button>("ExitButton").clicked += () => _viewModel?.Send(new TimelineSettingsOpenIntent());
    }
}

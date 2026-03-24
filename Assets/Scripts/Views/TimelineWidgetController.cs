using System.Collections;
using Assets.Scripts.States;
using Assets.Scripts.Utility.MVI;
using UnityEngine;
using UnityEngine.UIElements;

public class TimelineWidgetController : BaseUIController, IView<ParameterTimelineState, TimelineState>
{
    private VisualElement m_TimelineDrawer;
    private VisualElement m_Timeline;
    private TimelineSliderElement m_TimelineSlider;
    private Button m_OpenButton;

    private bool m_widgetOpen = false;

    private Button m_PlayButton;
    bool isPlaybackRunning = false;
    private float _playbackInterval;
    private int _maxFrames;

    private IViewModel<ParameterTimelineState, TimelineState> _viewModel;

    protected override void Awake()
    {
        base.Awake();

        m_TimelineDrawer = ui.Q<VisualElement>("TimelineDrawer");
        m_Timeline = ui.Q<VisualElement>("Timeline");
        m_OpenButton = ui.Q<Button>("TimelineOpenButton");
        m_PlayButton = ui.Q<Button>("PlayBtn");
        m_TimelineSlider = ui.Q<TimelineSliderElement>("TimelineSlider");
    }

    void Start()
    {
        m_OpenButton.RegisterCallback<ClickEvent>(OnButtonClick);

        // close timeline widget
        m_OpenButton.RemoveFromClassList("rotate");
        m_TimelineDrawer.AddToClassList("close-timeline");
        m_TimelineDrawer.RemoveFromClassList("open-timeline");
        m_Timeline.AddToClassList("hide");

        m_PlayButton.clicked += OnPlayButtonClicked;
    }

    /// <summary>
    /// Opening and closing logic of the widget.
    /// </summary>
    private void OnButtonClick(ClickEvent evt)
    {
        _viewModel?.Send(new TimelineOpenIntent());
    }

    public int Currentframe => m_TimelineSlider.CurrentFrame;

    /// <summary>
    /// Handle play function of the timeline.
    /// </summary>
    private void OnPlayButtonClicked()
    {
        if (isPlaybackRunning)
        {
            StopCoroutine(Playback());
            isPlaybackRunning = false;
        }
        else
        {
            StartCoroutine(Playback());
            isPlaybackRunning = true;
        }
    }

    IEnumerator Playback()
    {
        do
        {
            m_TimelineSlider.CurrentFrame = Currentframe >= _maxFrames ? 1 : Currentframe + 1;
            _viewModel?.Send(new CurrentFrameChangedIntent(Currentframe));
            yield return new WaitForSecondsRealtime(_playbackInterval);
        } while (isPlaybackRunning);
    }

    public void SetMaxFrames(int maxFrames)
    {
        _maxFrames = maxFrames;
    }

    public void SetFramePerSec(int framePerSec)
    {
        _playbackInterval = 1f / framePerSec;
    }

    private void CloseTimeline()
    {
        if (!m_widgetOpen) return;

        m_TimelineDrawer.AddToClassList("close-timeline");
        m_TimelineDrawer.RemoveFromClassList("open-timeline");
        m_OpenButton.RemoveFromClassList("rotate");
        m_Timeline.AddToClassList("hide");
        m_widgetOpen = false;
    }

    private void OpenTimeline(TimelineState state)
    {
        if (!m_widgetOpen)
        {
            m_TimelineDrawer.RemoveFromClassList("close-timeline");
            m_TimelineDrawer.AddToClassList("open-timeline");
            m_OpenButton.AddToClassList("rotate");
            m_Timeline.RemoveFromClassList("hide");
            m_widgetOpen = true;
        }
        SetMaxFrames(state.MaxFrames);
        SetFramePerSec(state.FramePerSec);

    }

    public void Render(TimelineState state)
    {
        if (state.IsOpen)
        {
            OpenTimeline(state);
        }
        else
        {
            CloseTimeline();
        }
    }
    public void SetViewModel(IViewModel<ParameterTimelineState, TimelineState> viewModel)
    {
        _viewModel = viewModel;
        _viewModel?.Bind(this);

        m_TimelineSlider.SetViewModel(_viewModel);
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class TimelineWidgetController : BaseUIController
{
    private VisualElement m_TimelineDrawer;
    private VisualElement m_Timeline;
    private TimelineSliderElement m_TimelineSlider;
    private Button m_OpenButton;

    private bool m_widgetOpen = false;

    private Button m_PlayButton;
    bool isPlaybackRunning = false;

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

        UIEvents.TimelineParameterSliderChanged += OnKeySliderChanged;
    }

    void OnDisable()
    {
        UIEvents.TimelineParameterSliderChanged -= OnKeySliderChanged;
    }

    /// <summary>
    /// When any of the slider of each parameter is changed, a keyframe is placed at the current frame and interpolates the meshes accordingly.
    /// </summary>
    /// <param name="id">Which parameter the slider is connected to</param>
    /// <param name="value">Slider value</param>
    private void OnKeySliderChanged(Guid id, float value)
    {
        AnimationManager.Instance.InterpolateParameter(value, id);
        KeyFrame newKeyframe = AnimationManager.Instance.CreateKeyframe(id, value);
        m_TimelineSlider.CreateKeyframeAtCurrentFrame(id, newKeyframe);
    }

    public void LoadKeyframes(KeyFrame[] keyframes)
    {
        m_TimelineSlider.LoadKeyframes(keyframes);
    }

    /// <summary>
    /// Opening and closing logic of the widget.
    /// </summary>
    private void OnButtonClick(ClickEvent evt)
    {
        if (m_widgetOpen)
        {
            m_TimelineDrawer.AddToClassList("close-timeline");
            m_TimelineDrawer.RemoveFromClassList("open-timeline");
            m_OpenButton.RemoveFromClassList("rotate");
            m_Timeline.AddToClassList("hide");
        }
        else
        {
            m_TimelineDrawer.RemoveFromClassList("close-timeline");
            m_TimelineDrawer.AddToClassList("open-timeline");
            m_OpenButton.AddToClassList("rotate");
            m_Timeline.RemoveFromClassList("hide");
        }

        m_widgetOpen = !m_widgetOpen;

        ParameterManager.Instance.ParameterWidgetVisibility = !m_widgetOpen;
        if (m_widgetOpen)
        {
            SendParametersToTimeline();
            SetMaxFrames(GeneralSettings.Instance.MaxFrames);
            SetFramePerSec(GeneralSettings.Instance.FramePerSec);
        }
    }


    /// <summary>
    /// Populate the timeline with the available parameters.
    /// </summary>
    private void SendParametersToTimeline()
    {
        List<Parameter> parameters = ParameterRegistry.Instance.GetAll().ToList();
        m_TimelineSlider.ClearKeyframeContainer();

        foreach (Parameter parameter in parameters)
        {
            List<KeyFrame> keyFrames = KeyFrameRegistry.Instance.GetKeyFramesOfParam(parameter.ID);
            if (!AnimationManager.Instance.GetCurrentCurveSliderValue(parameter.ID, out float paramValue))
            {
                paramValue = parameter.DefaultValue;
            }

            m_TimelineSlider.CreateKeyFrameLine(parameter, paramValue, keyFrames);
        }

        m_TimelineSlider.UpdateKeyWidth();

        // m_TimelineSlider.LoadParameters(parameters);
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
            m_TimelineSlider.CurrentFrame = Currentframe >= GeneralSettings.Instance.MaxFrames ? 1 : Currentframe + 1;
            yield return new WaitForSecondsRealtime(1f / GeneralSettings.Instance.FramePerSec);
        } while (isPlaybackRunning);
    }

    public void SetMaxFrames(int maxFrames)
    {
        GeneralSettings.Instance.MaxFrames = maxFrames;
        m_TimelineSlider.Redraw();
        SendParametersToTimeline();
    }

    public void SetFramePerSec(int framePerSec)
    {
        GeneralSettings.Instance.FramePerSec = framePerSec;
    }
}

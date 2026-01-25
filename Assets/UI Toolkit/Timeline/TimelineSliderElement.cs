using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.States;
using Assets.Scripts.Utility.MVI;
using UnityEngine;
using UnityEngine.UIElements;

[UxmlElement]
public partial class TimelineSliderElement : VisualElement, IView<TimelineState>
{
    private float _sliderContainerWidth;
    private float _sliderWidth;
    private int m_maxFrames = GeneralSettings.Instance.MaxFrames;

    private int m_currentFrame = 1;
    public int CurrentFrame
    {
        get => m_currentFrame;
        set
        {
            int newValue = value;
            if (newValue >= m_maxFrames)
            {
                newValue = m_maxFrames;
            }
            else if (newValue <= 0)
            {
                newValue = 1;
            }

            m_currentFrame = newValue;
            _viewModel?.Send(new CurrentFrameChangedIntent(CurrentFrame));
        }
    }

    private bool m_sliderGrabbed = false;
    IntegerField m_currentFrameField;
    Label m_Label;
    VisualElement m_sliderContainer;
    VisualElement m_sliderHandle;
    VisualElement m_topSectionRight;
    ScrollView m_keyframeContainer;
    Dictionary<Guid, KeyframeLineElement> m_keyframeLineElements = new();

    List<List<VisualElement>> m_frameBars;

    private IViewModel<TimelineState> _viewModel;

    private bool m_widgetOpen = false;

    public TimelineSliderElement()
    {
        m_topSectionRight = new();
        m_topSectionRight.AddToClassList("top-section-right");

        VisualElement topSectionLeft = new();
        topSectionLeft.AddToClassList("top-section-left");

        Button playButton = new();
        playButton.name = "PlayBtn";
        playButton.AddToClassList("play-button");
        topSectionLeft.Add(playButton);

        VisualElement topSection = new();
        topSection.AddToClassList("top-section");
        topSection.Add(topSectionLeft);
        topSection.Add(m_topSectionRight);
        Add(topSection);

        BuildFrameBars(m_maxFrames);

        CreateHeader();
        CreateSlider();
        CreateKeyframeContainers();
    }

    private void BuildFrameBars(int maxFrames)
    {
        m_frameBars ??= new List<List<VisualElement>>();

        if (m_frameBars.Count >= maxFrames)
        {
            // go from backwards and set style display to flex until its hidden
            for (int i = m_frameBars.Count - 1; i >= 0; i--)
            {
                if (m_frameBars[i][0].style.display == DisplayStyle.Flex)
                    break;

                foreach (var key in m_frameBars[i])
                {
                    key.style.display = DisplayStyle.Flex;
                }
            }
        }

        for (int i = m_frameBars.Count; i < maxFrames; i++)
        {
            m_frameBars.Add(new List<VisualElement>());
        }

        if (m_frameBars.Count > maxFrames)
        {
            // set display to hide in outside frames
            for (int i = maxFrames; i < m_frameBars.Count; i++)
            {
                foreach (var key in m_frameBars[i])
                {
                    key.style.display = DisplayStyle.None;
                }
            }
        }
    }

    int previousHighlightIdx = 0;
    private void HighlightBarAt(int frame)
    {
        foreach (var key in m_frameBars[previousHighlightIdx])
        {
            key.RemoveFromClassList("highlight-cell");
        }

        foreach (var key in m_frameBars[frame - 1])
        {
            key.AddToClassList("highlight-cell");
        }

        previousHighlightIdx = frame - 1;
    }


    private void CreateKeyframeContainers()
    {
        m_keyframeContainer = new();
        m_keyframeContainer.AddToClassList("keyframe-container");
        m_keyframeContainer.verticalScrollerVisibility = ScrollerVisibility.Hidden;

        this.Add(m_keyframeContainer);
    }

    public void ClearKeyframeContainer()
    {
        foreach (var line in m_keyframeLineElements.Values)
        {
            _viewModel?.Unbind(line);
        }
        m_keyframeContainer.Clear();
        m_keyframeLineElements.Clear();
        ClearFrameBarLists();
    }

    public void CreateKeyFrameLine(Guid paramID)
    {
        var keyframeLine = ViewFactory.Instance.CreateView<KeyframeLineElement, TimelineState>(m_maxFrames, m_frameBars, paramID);
        m_keyframeContainer.Add(keyframeLine);
        m_keyframeLineElements.Add(paramID, keyframeLine);
    }

    public void LoadKeyframes(KeyFrame[] keyframes)
    {
        m_keyframeLineElements.Clear();
        ClearFrameBarLists();

        foreach (KeyFrame key in keyframes)
        {
            m_keyframeLineElements[key.ParamID].InsertKeyframeAt(key.Frame, key.ID);
        }
    }

    public void UpdateKeyWidth()
    {
        foreach (List<VisualElement> bars in m_frameBars)
        {
            foreach (VisualElement key in bars)
            {
                key.style.width = _sliderWidth;
            }
        }
    }

    private void ClearFrameBarLists()
    {
        foreach (var bar in m_frameBars)
        {
            bar.Clear();
        }
    }

    private void RecalculateSliderHandle()
    {
        _sliderContainerWidth = m_sliderContainer.resolvedStyle.width;
        _sliderWidth = _sliderContainerWidth / m_maxFrames;
        m_sliderHandle.style.width = _sliderWidth;

        UpdateKeyWidth();
    }

    /// <summary>
    /// Create slider element and register input callbacks.
    /// </summary>
    private void CreateSlider()
    {
        m_sliderContainer = new();
        m_sliderContainer.AddToClassList("slider-container");

        m_sliderHandle = new();
        m_sliderHandle.AddToClassList("slider-handle");
        m_sliderHandle.RegisterCallbackOnce<GeometryChangedEvent>(
            (evt) =>
            {
                RecalculateSliderHandle();
            }
        );

        m_sliderContainer.RegisterCallback<PointerUpEvent>(OnDropHandle);
        m_sliderContainer.RegisterCallback<PointerDownEvent>(OnGrabHandle);


        m_sliderContainer.RegisterCallback<PointerMoveEvent>(OnMoveHandle);

        m_sliderContainer.Add(m_sliderHandle);

        m_topSectionRight.Add(m_sliderContainer);
    }

    private void OnMoveHandle(PointerMoveEvent evt)
    {
        if (m_sliderGrabbed)
        {
            FrameFromPointer(evt.localPosition.x);
        }
    }

    private void OnDropHandle(PointerUpEvent evt)
    {
        m_sliderGrabbed = false;
    }

    private void OnGrabHandle(PointerDownEvent evt)
    {
        m_sliderGrabbed = true;
        FrameFromPointer(evt.localPosition.x);
    }

    /// <summary>
    /// Determine currently selected frame from the position of the cursor.
    /// </summary>
    /// <param name="x">Cursor's x position</param>
    private void FrameFromPointer(float x)
    {
        if (_sliderContainerWidth <= 0) return;

        int newFrame = (int)Math.Round(x / _sliderContainerWidth * (m_maxFrames + 1));
        if (CurrentFrame != newFrame)
        {
            CurrentFrame = newFrame;
        }
    }

    /// <summary>
    /// Position the handle in the slider according to the current frame value.
    /// </summary>
    private void UpdateHandlePosition()
    {
        float x = (CurrentFrame - 1) * _sliderWidth;
        m_sliderHandle.style.left = Mathf.Clamp(x, 0, _sliderContainerWidth - _sliderWidth);
    }

    private void CreateHeader()
    {
        VisualElement header = new();
        header.AddToClassList("header");
        m_Label = new Label("Timeline");

        m_currentFrameField = new IntegerField();
        m_currentFrameField.AddToClassList("slider-inputfield");
        m_currentFrameField.value = 1;

        Button deleteKeyframeButton = new() { text = "Delete Keyframe" };
        deleteKeyframeButton.AddToClassList("delete-keyframe-button");
        deleteKeyframeButton.clicked += () =>
        {
            _viewModel?.Send(new DeleteKeyframeIntent());
        };

        Button settingsButton = new() { text = "Settings" };
        settingsButton.AddToClassList("delete-keyframe-button");
        settingsButton.clicked += () =>
        {
            Debug.Log("settings clicked");
            _viewModel?.Send(new TimelineSettingsOpenIntent());
        };

        header.Add(m_Label);
        header.Add(settingsButton);
        header.Add(deleteKeyframeButton);
        header.Add(m_currentFrameField);

        m_topSectionRight.Add(header);

        m_currentFrameField.RegisterCallback<FocusOutEvent>(OnFrameChanged);
    }

    private void OnFrameChanged(FocusOutEvent evt)
    {
        if (m_currentFrameField.value <= 0)
        {
            m_currentFrameField.value = 0;
            return;
        }
        else if (m_currentFrameField.value > m_maxFrames)
        {
            m_currentFrameField.value = m_maxFrames;
            return;
        }

        CurrentFrame = m_currentFrameField.value;
    }

    private void UpdateFrameField()
    {
        m_currentFrameField.value = CurrentFrame;
    }

    private void RebuildKeyFrameLines(TimelineState state)
    {
        ClearKeyframeContainer();

        foreach (var (id, _) in state.Keyframes)
        {
            if (!m_keyframeLineElements.ContainsKey(id))
                CreateKeyFrameLine(id);
        }

        UpdateKeyWidth();
    }

    public void Render(TimelineState state)
    {
        if (state.IsOpen && (!m_widgetOpen || m_maxFrames != state.MaxFrames))
        {
            RebuildKeyFrameLines(state);
        }

        if (m_maxFrames != state.MaxFrames)
        {
            m_maxFrames = state.MaxFrames;
            BuildFrameBars(state.MaxFrames);
            RecalculateSliderHandle();
        }

        UpdateHandlePosition();
        HighlightBarAt(state.CurrentFrame);
        UpdateFrameField();

        m_widgetOpen = state.IsOpen;
    }

    public void SetViewModel(IViewModel<TimelineState> viewModel)
    {
        _viewModel = viewModel;
        _viewModel?.Bind(this);
    }
}

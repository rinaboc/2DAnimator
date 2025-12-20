using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[UxmlElement]
public partial class TimelineSliderElement : VisualElement
{
    private float _sliderContainerWidth;
    private float _sliderWidth;
    private int m_maxFrames = 24;

    public int MaxFrames
    {
        get => m_maxFrames;
        set
        {
            if (value == m_maxFrames) return;
            m_maxFrames = value;
            Debug.Log("max frames changed" + value);
        }
    }
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
            UpdateHandlePosition();
            UpdateFrameField();
            HighlightBarAt(m_currentFrame);
            UIEvents.RaiseTimelineChange(CurrentFrame);
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

    List<VisualElement>[] m_frameBars;


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

        m_frameBars = new List<VisualElement>[m_maxFrames];
        for (int i = 0; i < m_maxFrames; i++)
        {
            m_frameBars[i] = new List<VisualElement>();
        }

        CreateHeader();
        CreateSlider();
        CreateKeyframeContainers();
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

    public void LoadParameters(List<Parameter> parameters)
    {
        m_keyframeContainer.Clear();
        m_keyframeLineElements.Clear();
        ClearFrameBarLists();

        foreach (Parameter parameter in parameters)
        {
            KeyframeLineElement keyframeLine = new(MaxFrames, m_frameBars, parameter);
            if (AnimationManager.Instance.GetCurrentCurveSliderValue(parameter.ID, out float paramValue))
            {
                keyframeLine.SetSliderValue(paramValue);
            }
            m_keyframeContainer.Add(keyframeLine);
            m_keyframeLineElements.Add(parameter.ID, keyframeLine);

            List<KeyFrame> keyFrames = KeyFrameRegistry.Instance.GetKeyFramesOfParam(parameter.ID);
            foreach (var key in keyFrames)
            {
                keyframeLine.InsertKeyframeAt(key.Frame, key);
            }
        }

        UpdateKeyWidth();
    }

    public void CreateKeyframeAtCurrentFrame(Guid paramID, KeyFrame key)
    {
        m_keyframeLineElements[paramID].InsertKeyframeAt(CurrentFrame, key);
    }

    public void LoadKeyframes(KeyFrame[] keyframes)
    {
        m_keyframeLineElements.Clear();
        ClearFrameBarLists();

        foreach (KeyFrame key in keyframes)
        {
            m_keyframeLineElements[key.ParamID].InsertKeyframeAt(key.Frame, key);
        }
    }

    private void UpdateKeyWidth()
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
        _sliderWidth = _sliderContainerWidth / MaxFrames;
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

        int newFrame = (int)Math.Round(x / _sliderContainerWidth * (MaxFrames + 1));
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
            UIEvents.RaiseDeleteSelectedKeyframe();
        };

        header.Add(m_Label);
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
        else if (m_currentFrameField.value > MaxFrames)
        {
            m_currentFrameField.value = MaxFrames;
            return;
        }

        CurrentFrame = m_currentFrameField.value;
    }

    private void UpdateFrameField()
    {
        m_currentFrameField.value = CurrentFrame;
    }
}

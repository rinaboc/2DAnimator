using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[UxmlElement]
public partial class TimelineSlider : VisualElement
{
    private float _sliderContainerWidth;
    private float _sliderWidth;
    private int m_maxFrames = 24; // TODO: custom max frame at runtime
    [UxmlAttribute]
    public int MaxFrames
    {
        get => m_maxFrames;
        set
        {
            m_maxFrames = value;
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
            AnimationManager.TimelineChangeEvent.Invoke(CurrentFrame);
        }
    }

    private bool m_sliderGrabbed = false;
    IntegerField m_currentFrameField;
    Label m_Label;
    VisualElement m_sliderContainer;
    VisualElement m_sliderHandle;
    VisualElement m_topSection;
    ScrollView m_keyframeContainer;
    Dictionary<Guid, KeyframeLineElement> m_keyframeLineElements = new();

    List<VisualElement>[] m_frameBars;

    public TimelineSlider()
    {
        m_topSection = new();
        m_topSection.AddToClassList("top-section");
        Add(m_topSection);

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
            if (AnimationManager.instance.GetParamCurValue(parameter.ID, out float paramValue))
            {
                keyframeLine.SetSliderValue(paramValue);
            }
            m_keyframeContainer.Add(keyframeLine);
            m_keyframeLineElements.Add(parameter.ID, keyframeLine);
        }

        UpdateKeyWidth();
    }

    public void CreateKeyframeAtCurrentFrame(Guid paramID, KeyFrame key)
    {
        m_keyframeLineElements[paramID].InsertKeyframeAt(CurrentFrame, key);
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
                _sliderContainerWidth = m_sliderContainer.resolvedStyle.width;
                _sliderWidth = _sliderContainerWidth / MaxFrames;
                m_sliderHandle.style.width = _sliderWidth;

                UpdateKeyWidth();
            }
        );

        m_sliderContainer.RegisterCallback<PointerUpEvent>(OnDropHandle);
        m_sliderContainer.RegisterCallback<PointerDownEvent>(OnGrabHandle);


        m_sliderContainer.RegisterCallback<PointerMoveEvent>(OnMoveHandle);

        m_sliderContainer.Add(m_sliderHandle);

        m_topSection.Add(m_sliderContainer);
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
            AnimationManager.DeleteSelectedKeyframeEvent.Invoke();
        };

        header.Add(m_Label);
        header.Add(deleteKeyframeButton);
        header.Add(m_currentFrameField);

        m_topSection.Add(header);

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

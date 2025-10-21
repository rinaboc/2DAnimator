using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[UxmlElement]
public partial class TimelineSlider : VisualElement
{
    private float _sliderContainerWidth;
    private float _sliderWidth;
    private int m_maxFrames = 24;
    [UxmlAttribute]
    public int MaxFrames
    {
        get => m_maxFrames;
        set
        {
            m_maxFrames = value;
        }
    }
    private int m_currentFrame = 0;
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
        }
    }

    private bool m_sliderGrabbed = false;
    IntegerField m_currentFrameField;
    Label m_Label;
    VisualElement m_sliderContainer;
    VisualElement m_sliderHandle;
    VisualElement m_topSection;

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
        ScrollView keyframeContainer = new();
        keyframeContainer.AddToClassList("keyframe-container");
        keyframeContainer.verticalScrollerVisibility = ScrollerVisibility.Hidden;

        for (int i = 0; i < 10; i++)
        {
            VisualElement keyframeLine = new();
            keyframeLine.AddToClassList("parameter-keys-container");
            keyframeLine.Add(new Label("parameter name"));

            for (int j = 1; j < MaxFrames; j++)
            {
                VisualElement key = new();
                key.AddToClassList("parameter-key");
                keyframeLine.Add(key);

                if (j == 1) key.AddToClassList("highlight-cell");

                m_frameBars[j - 1].Add(key);

                m_sliderHandle.RegisterCallbackOnce<GeometryChangedEvent>(
                (evt) =>
                {
                    key.style.width = _sliderWidth;
                }
                );
            }

            keyframeContainer.Add(keyframeLine);
        }

        this.Add(keyframeContainer);
    }

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

    private void FrameFromPointer(float x)
    {
        float width = _sliderContainerWidth;
        if (width <= 0) return;

        int newFrame = (int)Math.Round(x / width * (MaxFrames + 1));
        if (CurrentFrame != newFrame)
        {
            CurrentFrame = newFrame;
        }
    }

    private void UpdateHandlePosition()
    {
        float width = _sliderContainerWidth;

        float x = (CurrentFrame - 1) * _sliderWidth;
        m_sliderHandle.style.left = Mathf.Clamp(x, 0, width - _sliderWidth);
    }

    private void CreateHeader()
    {
        VisualElement header = new();
        header.AddToClassList("header");
        m_Label = new Label("Timeline");
        m_currentFrameField = new IntegerField();
        m_currentFrameField.AddToClassList("slider-inputfield");
        m_currentFrameField.value = 1;

        header.Add(m_Label);
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

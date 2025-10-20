using System;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[UxmlElement]
public partial class TimelineSlider : VisualElement
{
    private int m_maxFrames = 20;
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
            else if (newValue < 0)
            {
                newValue = 0;
            }

            m_currentFrame = newValue;
            UpdateHandlePosition();
            UpdateFrameField();
        }
    }

    private bool m_sliderGrabbed = false;
    IntegerField m_currentFrameField;
    Label m_Label;
    VisualElement m_sliderContainer;
    VisualElement m_sliderHandle;

    public TimelineSlider()
    {
        CreateHeader();
        CreateSlider();
        CreateKeyframeContainers();
    }

    private void CreateKeyframeContainers()
    {
        ScrollView keyframeContainer = new();
        keyframeContainer.AddToClassList("keyframe-container");

        this.Add(keyframeContainer);
    }

    private void CreateSlider()
    {
        m_sliderContainer = new();
        m_sliderContainer.AddToClassList("slider-container");

        m_sliderHandle = new();
        m_sliderHandle.AddToClassList("slider-handle");

        m_sliderHandle.RegisterCallback<PointerUpEvent>(OnDropHandle);
        m_sliderHandle.RegisterCallback<PointerDownEvent>(OnGrabHandle);

        this.RegisterCallback<PointerMoveEvent>(OnMoveHandle);

        m_sliderContainer.Add(m_sliderHandle);

        this.Add(m_sliderContainer);
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

        evt.StopPropagation();
    }

    private void FrameFromPointer(float x)
    {
        float width = m_sliderContainer.resolvedStyle.width;
        if (width <= 0) return;

        int newFrame = (int)Math.Round(x / width * m_maxFrames);
        if (CurrentFrame != newFrame)
        {
            CurrentFrame = newFrame;
        }
    }

    private void UpdateHandlePosition()
    {
        float width = m_sliderContainer.resolvedStyle.width;
        float handleWidth = m_sliderHandle.resolvedStyle.width;

        float x = (float)CurrentFrame / m_maxFrames * width - handleWidth * 0.5f;
        m_sliderHandle.style.left = Mathf.Clamp(x, 0, width - handleWidth);

        m_sliderHandle.MarkDirtyRepaint();
    }

    private void CreateHeader()
    {
        VisualElement header = new();
        header.AddToClassList("header");
        m_Label = new Label("Timeline");
        m_currentFrameField = new IntegerField();
        m_currentFrameField.AddToClassList("slider-inputfield");

        header.Add(m_Label);
        header.Add(m_currentFrameField);

        Add(header);

        m_currentFrameField.RegisterValueChangedCallback(OnFrameChanged);
    }

    private void OnFrameChanged(ChangeEvent<int> evt)
    {
        CurrentFrame = evt.newValue;
    }

    private void UpdateFrameField()
    {
        m_currentFrameField.value = CurrentFrame;
    }
}

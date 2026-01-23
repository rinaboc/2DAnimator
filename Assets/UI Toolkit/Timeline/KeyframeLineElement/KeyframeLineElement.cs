using System;
using System.Collections.Generic;
using Assets.Scripts.States;
using UnityEngine.UIElements;

[UxmlElement]
public partial class KeyframeLineElement : VisualElement
{
    public Guid ParamID { get; }
    private List<VisualElement> _cells = new();
    private ParameterSliderElement _paramSlider;
    public KeyframeLineElement()
    {
        AddToClassList("animation-cell-container");
    }

    public KeyframeLineElement(int MaxFrames, List<List<VisualElement>> frameBars, Parameter parameter) : this()
    {
        ParamID = parameter.ID;

        _paramSlider = ViewFactory.Instance.CreateView<ParameterSliderElement, ParameterStates>(ParamID);
        Add(_paramSlider);

        for (int j = 1; j <= MaxFrames; j++)
        {
            VisualElement cell = new();
            cell.AddToClassList("animation-cell");
            Add(cell);
            _cells.Add(cell);

            if (j == 1) cell.AddToClassList("highlight-cell");

            frameBars[j - 1].Add(cell);
        }
    }

    public void InsertKeyframeAt(int frame, KeyFrame key)
    {
        RemoveKeyframeFrom(frame);

        _cells[frame - 1].Add(new KeyframeElement(key, this));
    }

    public void RemoveKeyframeFrom(int frame)
    {
        VisualElement cell = _cells[frame - 1];
        if (cell.childCount > 0)
        {
            KeyframeElement keyframe = cell.Q<KeyframeElement>();
            keyframe.Delete();
            cell.Clear();
        }
    }
}

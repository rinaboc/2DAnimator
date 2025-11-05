using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
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

    public KeyframeLineElement(int MaxFrames, List<VisualElement>[] frameBars, Parameter parameter) : this()
    {
        ParamID = parameter.ID;

        _paramSlider = new ParameterSliderElement(parameter);
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

    public void SetSliderValue(float value)
    {
        _paramSlider.SetSliderValue(value);
    }

    public void InsertKeyframeAt(int frame, KeyFrame key)
    {
        RemoveKeyframeFrom(frame);

        _cells[frame - 1].Add(new KeyframeElement(key, this));
    }

    public void RemoveKeyframeFrom(int frame)
    {
        VisualElement cell = _cells[frame - 1];
        cell.Clear();
    }
}

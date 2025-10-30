using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[UxmlElement]
public partial class KeyframeLineElement : VisualElement
{
    public Guid ParamID { get; }
    private List<VisualElement> _cells = new();
    public KeyframeLineElement()
    {
        AddToClassList("animation-cell-container");
    }

    public KeyframeLineElement(int MaxFrames, List<VisualElement>[] frameBars, Parameter parameter) : this()
    {
        ParamID = parameter.ID;

        Add(new ParameterSliderElement(parameter));
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

    public void InsertKeyframeAt(int frame)
    {
        VisualElement cell = _cells[frame - 1];
        cell.Clear();

        VisualElement keyframe = new();
        keyframe.AddToClassList("keyframe");
        cell.Add(keyframe);
    }
}

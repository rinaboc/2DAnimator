using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[UxmlElement]
public partial class KeyframeLineElement : VisualElement
{
    public KeyframeLineElement()
    {
        AddToClassList("parameter-keys-container");
    }

    public KeyframeLineElement(int MaxFrames, List<VisualElement>[] frameBars, Parameter parameter) : this()
    {
        Add(new ParameterSliderElement(parameter));
        for (int j = 1; j <= MaxFrames; j++)
        {
            VisualElement key = new();
            key.AddToClassList("parameter-key");
            Add(key);

            if (j == 1) key.AddToClassList("highlight-cell");

            frameBars[j - 1].Add(key);
        }
    }
}

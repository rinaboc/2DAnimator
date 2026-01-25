using System;
using System.Collections.Generic;
using Assets.Scripts.States;
using Assets.Scripts.Utility.MVI;
using UnityEngine.UIElements;

[UxmlElement]
public partial class KeyframeLineElement : VisualElement, IView<TimelineState>, IDisposable
{
    public Guid ParamID { get; }
    private List<VisualElement> _cells = new();
    private ParameterSliderElement _paramSlider;
    private IViewModel<TimelineState> _viewModel;
    public KeyframeLineElement()
    {
        AddToClassList("animation-cell-container");
    }

    public KeyframeLineElement(int MaxFrames, List<List<VisualElement>> frameBars, Guid paramID) : this()
    {
        ParamID = paramID;

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

    public void Dispose()
    {
        _viewModel?.Unbind(this);
        foreach (var cell in _cells)
        {
            var key = cell.Q<KeyframeElement>();
            key?.Dispose();
            cell.Clear();
        }
    }


    public void InsertKeyframeAt(int frame, Guid keyID)
    {
        if (_cells[frame - 1].Q<KeyframeElement>()?._id == keyID) return;

        RemoveKeyframeFrom(frame);
        var keyframe = ViewFactory.Instance.CreateView<KeyframeElement, TimelineState>(keyID, frame, this);
        _cells[frame - 1].Add(keyframe);
    }

    public void RemoveKeyframeFrom(int frame)
    {
        VisualElement cell = _cells[frame - 1];
        if (cell.childCount > 0)
        {
            KeyframeElement keyframe = cell.Q<KeyframeElement>();
            keyframe.Dispose();
            cell.Clear();
        }
    }

    public void Render(TimelineState state)
    {
        for (int i = 0; i < _cells.Count; i++)
        {
            if (_cells[i].childCount > 0)
            {
                KeyframeElement keyframe = _cells[i].Q<KeyframeElement>();
                if (!state.Keyframes[ParamID].ContainsKey(keyframe._id))
                {
                    keyframe.Dispose();
                    _cells[i].Clear();
                }
            }
        }

        foreach (var keyframe in state.Keyframes[ParamID])
        {
            InsertKeyframeAt(keyframe.Value.Frame, keyframe.Key);
        }
    }

    public void SetViewModel(IViewModel<TimelineState> viewModel)
    {
        _viewModel = viewModel;
        _viewModel?.Bind(this);
    }
}

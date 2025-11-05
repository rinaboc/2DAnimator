using System;
using UnityEngine;
using UnityEngine.UIElements;

[UxmlElement]
public partial class KeyframeElement : VisualElement
{
    private Guid _id;
    private int _frame;
    private bool _isSelected = false;
    private KeyframeLineElement _parentElement;

    public KeyframeElement()
    {
        AddToClassList("keyframe");
    }

    public KeyframeElement(KeyFrame key, KeyframeLineElement parentElement) : this()
    {
        _parentElement = parentElement;
        _id = key.ID;
        _frame = key.Frame;
        RegisterCallback<ClickEvent>(OnClick);
        AnimationManager.SelectKeyframeEvent.AddListener(OnKeyframeSelect);
        AnimationManager.DeleteSelectedKeyframeEvent.AddListener(OnDeleteKeyframe);
    }

    private void OnKeyframeSelect(Guid id)
    {
        _isSelected = id.Equals(_id);
        if (_isSelected)
        {
            AddToClassList("selected-keyframe");
        }
        else
        {
            RemoveFromClassList("selected-keyframe");
        }
    }

    ~KeyframeElement()
    {
        UnregisterCallback<ClickEvent>(OnClick);
        AnimationManager.SelectKeyframeEvent.RemoveListener(OnKeyframeSelect);
        AnimationManager.DeleteSelectedKeyframeEvent.RemoveListener(OnDeleteKeyframe);
    }

    private void OnDeleteKeyframe()
    {
        if (!_isSelected) return;

        _parentElement.RemoveKeyframeFrom(_frame);
        AnimationManager.instance.RemoveKeyFrame(_id);
    }

    private void OnClick(ClickEvent evt)
    {
        if (_isSelected) return;

        AnimationManager.SelectKeyframeEvent.Invoke(_id);
    }
}

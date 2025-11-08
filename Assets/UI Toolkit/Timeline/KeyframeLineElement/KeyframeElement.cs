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
        UIEvents.SelectKeyframeEvent.AddListener(OnKeyframeSelect);
        UIEvents.DeleteSelectedKeyframeEvent.AddListener(OnDeleteKeyframe);
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
        UIEvents.SelectKeyframeEvent.RemoveListener(OnKeyframeSelect);
        UIEvents.DeleteSelectedKeyframeEvent.RemoveListener(OnDeleteKeyframe);
    }

    private void OnDeleteKeyframe()
    {
        if (!_isSelected) return;

        _parentElement.RemoveKeyframeFrom(_frame);
        AnimationManager.Instance.RemoveKeyFrame(_id);
    }

    private void OnClick(ClickEvent evt)
    {
        if (_isSelected) return;

        UIEvents.SelectKeyframeEvent.Invoke(_id);
    }
}

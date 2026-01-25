using System;
using Assets.Scripts.States;
using Assets.Scripts.Utility.MVI;
using UnityEngine;
using UnityEngine.UIElements;

[UxmlElement]
public partial class KeyframeElement : VisualElement, IView<TimelineState>
{
    public Guid _id;
    private int _frame;
    private bool _isSelected = false;
    private KeyframeLineElement _parentElement;
    private IViewModel<TimelineState> _viewModel;

    public KeyframeElement()
    {
        AddToClassList("keyframe");
    }

    public KeyframeElement(Guid keyID, int frame, KeyframeLineElement parentElement) : this()
    {
        _parentElement = parentElement;
        _id = keyID;
        _frame = frame;
        RegisterCallback<ClickEvent>(OnClick);
    }

    ~KeyframeElement()
    {
        UnregisterCallback<ClickEvent>(OnClick);
        _viewModel?.Unbind(this);
    }

    private void OnClick(ClickEvent evt)
    {
        if (_isSelected) return;
        _viewModel?.Send(new SelectKeyframeIntent(_id));
    }

    public void Render(TimelineState state)
    {
        if (!state.Keyframes[_parentElement.ParamID].ContainsKey(_id)) { Debug.Log("zombie keyframe"); return; }
        _isSelected = state.Keyframes[_parentElement.ParamID][_id].IsSelected;
        if (_isSelected)
        {
            AddToClassList("selected-keyframe");
        }
        else
        {
            RemoveFromClassList("selected-keyframe");
        }
    }

    public void SetViewModel(IViewModel<TimelineState> viewModel)
    {
        _viewModel = viewModel;
        _viewModel?.Bind(this);
    }
}

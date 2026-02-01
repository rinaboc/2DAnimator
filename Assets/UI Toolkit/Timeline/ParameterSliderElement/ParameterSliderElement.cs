using System;
using System.Collections.Generic;
using Assets.Scripts.States;
using Assets.Scripts.Utility.MVI;
using UnityEngine;
using UnityEngine.UIElements;

[UxmlElement]
public partial class ParameterSliderElement : VisualElement, IView<ParameterStates, ParameterStates>
{
    public Guid _paramID { get; set; }
    Slider _slider;
    Label _label;
    VisualElement _container;
    VisualElement _handle;

    private IViewModel<ParameterStates, ParameterStates> _viewModel;

    public ParameterSliderElement()
    {
        _label = new("parameter name");
        _label.AddToClassList("param-name");
        Add(_label);

        _slider = new();
        _slider.RegisterValueChangedCallback(OnSliderChange);
        Add(_slider);

        _slider.fill = true;

        _container = _slider.Q<VisualElement>("unity-drag-container");
        _container.AddToClassList("param-slider-container");

        _handle = _slider.Q<VisualElement>("unity-dragger");
        _handle.AddToClassList("param-slider-handle");
    }

    private void OnSliderChange(ChangeEvent<float> evt)
    {
        _viewModel?.Send(new TimelineParameterSliderChangedIntent(_paramID, evt.newValue, Guid.NewGuid()));
    }

    public ParameterSliderElement(Guid ID) : this()
    {
        _paramID = ID;
    }

    private void AddKeys(List<float> values)
    {
        var paramKeys = new List<Button>();
        foreach (var child in _container.Children())
        {
            if (child is Button paramKey) paramKeys.Add(paramKey);
        }

        for (int i = 0; i < values.Count; i++)
        {
            float value = values[i];
            if (paramKeys.Count > i)
            {
                Button paramKey = paramKeys[i];
                PositionParamKeyAt(value, paramKey);
            }
            else
            {
                Button paramKey = new();
                paramKey.AddToClassList("key-param-slider-handle");
                _container.Add(paramKey);
                _container.RegisterCallback<GeometryChangedEvent>(evt =>
                {
                    PositionParamKeyAt(value, paramKey);
                });
                paramKey.clicked += () =>
                {
                    _slider.value = value;
                };
            }

            _handle.BringToFront();
        }
    }

    /// <summary>
    /// Position the parameterKey button at the set value on the slider.
    /// </summary>
    private void PositionParamKeyAt(float value, Button paramKey)
    {
        float range = _slider.highValue - _slider.lowValue;
        float normalized = range > 0f ? (value - _slider.lowValue) / range : 0f;
        normalized = Mathf.Clamp01(normalized);

        float usableWidth = _container.layout.width;
        float xPos = usableWidth * normalized;

        float buttonWidth = paramKey.layout.width;
        if (float.IsNaN(buttonWidth) || buttonWidth <= 0f)
            buttonWidth = paramKey.resolvedStyle.width;

        float left = Mathf.Clamp(xPos - (buttonWidth / 2f), 0f, Mathf.Max(0f, usableWidth - buttonWidth));

        paramKey.style.position = Position.Absolute;
        paramKey.style.left = left;
        paramKey.style.top = 0;
    }

    public void Render(ParameterStates state)
    {
        if (_paramID == Guid.Empty) return;

        Debug.Log("parameter slider draw");

        var parameter = state.Parameters[_paramID];
        _label.text = parameter.Name;
        _slider.lowValue = parameter.MinValue;
        _slider.highValue = parameter.MaxValue;
        _slider.SetValueWithoutNotify(parameter.CurValue);

        AddKeys(parameter.ParamPointValues);
    }

    public void SetViewModel(IViewModel<ParameterStates, ParameterStates> viewModel)
    {
        _viewModel = viewModel;
        _viewModel?.Bind(this);
    }
}

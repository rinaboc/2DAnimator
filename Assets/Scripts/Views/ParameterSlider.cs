using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.States;
using Assets.Scripts.Utility.MVI;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ParameterSlider : Clickable, IView<ParameterTimelineState, ParameterStates>
{
    private Guid paramID;

    private InputAction clickAction;

    [Header("Parameter selection")]
    [SerializeField] private Color SelectedColor;
    [SerializeField] private Color DefaultColor;
    [SerializeField] private Image Background;
    [SerializeField] private TMP_Text ParameterNameText;
    [SerializeField] private TMP_InputField ParameterValueField;
    private InputAction DoubleClickAction;

    [Header("Parameter Curve Assignment")]
    [SerializeField] private Slider slider;
    [SerializeField] private Color CurveNormalColor;
    [SerializeField] private Color CurveAssignedColor;
    [SerializeField] private GameObject ParamPointPrefab;
    [SerializeField] private Transform SlideArea;

    private readonly Dictionary<float, GameObject> paramPoints = new();

    private float sliderValue;
    private string paramName;
    private bool _isSelected = false;

    private IViewModel<ParameterTimelineState, ParameterStates> _viewModel;

    void OnEnable()
    {
        clickAction = InputSystem.actions.FindAction("Click");
        clickAction.performed += OnClick;

        DoubleClickAction = InputSystem.actions.FindAction("DoubleClick");
        DoubleClickAction.performed += OnDoubleClick;

        ParameterValueField.onSubmit.AddListener((context) =>
        {
            if (float.TryParse(ParameterValueField.text, out float input))
                SetValue(input);
        });
    }

    public void OnDragStart()
    {
        _viewModel?.Send(new StartParameterDragIntent(paramID, slider.value));
    }

    private void OnDestroy()
    {
        DoubleClickAction.performed -= OnDoubleClick;
        clickAction.performed -= OnClick;

        _viewModel?.Unbind(this);
    }

    private void OnDoubleClick(InputAction.CallbackContext context)
    {
        if (IsInsideCollider())
        {
            _viewModel?.Send(new OpenParameterEditorIntent());
        }
    }

    public void SetParamName(string name)
    {
        paramName = name;
        ParameterNameText.text = paramName;
    }

    public void SetParamID(Guid paramID)
    {
        this.paramID = paramID;
    }

    void OnClick(InputAction.CallbackContext context)
    {
        if (IsInsideCollider() && !_isSelected)
        {
            _viewModel?.Send(new SelectParameterIntent(paramID));
        }
    }

    public void SetSelected(bool isSelected)
    {
        Background.color = isSelected ? SelectedColor : DefaultColor;
        _isSelected = isSelected;
    }

    private void SetMinMaxValues(float min, float max)
    {
        slider.minValue = min;
        slider.maxValue = max;
    }

    public void SetAssignedCurve(bool isAssigned)
    {
        ColorBlock colorBlock = slider.colors;
        colorBlock.normalColor = isAssigned ? CurveAssignedColor : CurveNormalColor;

        slider.colors = colorBlock;

        foreach ((_, GameObject item) in paramPoints)
        {
            item.GetComponent<Image>().color = isAssigned ? CurveAssignedColor : CurveNormalColor;
        }
    }

    public void CreateParamPointHandles(List<float> values)
    {
        float maxValue = values.Max();
        float minValue = values.Min();
        foreach (float value in values)
        {
            float key = Mathf.Round(value * 100f) / 100f;
            if (paramPoints.ContainsKey(key)) continue;

            GameObject pointHandle = Instantiate(ParamPointPrefab, SlideArea);
            RectTransform pointTransform = pointHandle.GetComponent<RectTransform>();
            float normalizedPoint = (key - minValue) / (maxValue - minValue);
            pointTransform.anchorMax = pointTransform.anchorMin = new Vector2(normalizedPoint, pointTransform.anchorMin.y);
            pointHandle.transform.SetAsFirstSibling();

            paramPoints.TryAdd(key, pointHandle);

        }

        SetMinMaxValues(minValue, maxValue);
    }

    public void DeleteParamPointHandles()
    {
        foreach ((_, GameObject item) in paramPoints)
        {
            Destroy(item);
        }
        paramPoints.Clear();
    }

    public void OnValueChanged()
    {
        if (Math.Abs(sliderValue - slider.value) < 0.01f) return;
        _viewModel?.Send(new InterpolateParameterIntent(paramID, slider.value));
    }

    public void SetValue(float value)
    {
        sliderValue = value;
        slider.value = value;
        ParameterValueField.text = sliderValue.ToString("F2");
    }

    public void SetValueWithoutNotify(float value)
    {
        sliderValue = value;
        slider.SetValueWithoutNotify(value);
        ParameterValueField.text = sliderValue.ToString("F2");
    }

    public float GetValue()
    {
        return sliderValue;
    }

    public void Render(ParameterStates state)
    {
        if (!state.Parameters.TryGetValue(paramID, out var parameterState)) return;

        SetSelected(parameterState.IsSelected);
        SetParamName(parameterState.Name);
        SetMinMaxValues(parameterState.MinValue, parameterState.MaxValue);
        SetValue(parameterState.CurValue);
    }

    public void SetViewModel(IViewModel<ParameterTimelineState, ParameterStates> viewModel)
    {
        _viewModel = viewModel;
        _viewModel?.Bind(this);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ParameterSlider : Clickable, ISelectable
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

    private readonly List<GameObject> paramPoints = new();

    private float sliderValue;
    private string paramName;

    protected override void Start()
    {
        base.Start();
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

    private void OnDoubleClick(InputAction.CallbackContext context)
    {
        if (IsInsideCollider())
        {
            ParameterManager.instance.StartParameterEditing(paramID);
        }
    }

    private void OnDestroy()
    {
        DoubleClickAction.Dispose();
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
        if (IsInsideCollider())
        {
            ParameterManager.instance.SelectParameter(paramID);
        }
    }

    public void SetSelected(bool isSelected)
    {
        Background.color = isSelected ? SelectedColor : DefaultColor;
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

        foreach (GameObject item in paramPoints)
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
            GameObject pointHandle = Instantiate(ParamPointPrefab, SlideArea);
            RectTransform pointTransform = pointHandle.GetComponent<RectTransform>();
            float normalizedPoint = (value - minValue) / (maxValue - minValue);
            pointTransform.anchorMax = pointTransform.anchorMin = new Vector2(normalizedPoint, pointTransform.anchorMin.y);
            pointHandle.transform.SetAsFirstSibling();

            paramPoints.Add(pointHandle);
        }

        SetMinMaxValues(minValue, maxValue);
    }

    public void OnValueChanged()
    {
        sliderValue = slider.value;
        ParameterValueField.text = sliderValue.ToString("F1");
        AnimationManager.instance.InterpolateParameter(sliderValue, paramID);
    }

    public void SetValue(float value)
    {
        sliderValue = value;
        slider.value = value;
        ParameterValueField.text = sliderValue.ToString();
    }

    public float GetValue()
    {
        // return (int)Math.Round(sliderValue, MidpointRounding.AwayFromZero);
        return sliderValue;
    }

    public void UpdateSlider(Parameter parameter)
    {
        SetParamName(parameter.Name);
        SetMinMaxValues(parameter.MinValue, parameter.MaxValue);
        SetValue(parameter.DefaultValue);
    }
}

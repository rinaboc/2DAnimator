using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ParameterSlider : Clickable, ISelectable
{
    private ushort paramID;

    private InputAction clickAction;

    [Header("Parameter selection")]
    [SerializeField] private Color SelectedColor;
    [SerializeField] private Color DefaultColor;
    [SerializeField] private Image Background;

    [Header("Parameter Curve Assignment")]
    [SerializeField] private Slider slider;
    [SerializeField] private Color CurveNormalColor;
    [SerializeField] private Color CurveAssignedColor;
    [SerializeField] private GameObject ParamPointPrefab;
    [SerializeField] private Transform SlideArea;

    private readonly List<GameObject> paramPoints = new();

    private float sliderValue;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start()
    {
        base.Start();
        clickAction = InputSystem.actions.FindAction("Click");
        clickAction.performed += OnClick;
    }

    public void SetParamID(ushort paramID)
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

    public void CreateParamPointHandles(List<int> values)
    {
        foreach (int value in values)
        {
            GameObject pointHandle = Instantiate(ParamPointPrefab, SlideArea);
            RectTransform pointTransform = pointHandle.GetComponent<RectTransform>();
            pointTransform.anchorMax = pointTransform.anchorMin = new Vector2(value, pointTransform.anchorMin.y);
            pointHandle.transform.SetAsFirstSibling();

            paramPoints.Add(pointHandle);
        }
    }

    public void OnValueChanged()
    {
        sliderValue = slider.value;
        Debug.Log("value changed");
    }

    public int GetValue()
    {
        return (int)Math.Round(sliderValue, MidpointRounding.AwayFromZero);
    }
}

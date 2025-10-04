using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ParameterSlider : Clickable, ISelectable
{
    private ushort paramID;

    private InputAction clickAction;
    [SerializeField] private Color SelectedColor;
    [SerializeField] private Color DefaultColor;
    [SerializeField] private Image Background;
    [SerializeField] private Slider slider;

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

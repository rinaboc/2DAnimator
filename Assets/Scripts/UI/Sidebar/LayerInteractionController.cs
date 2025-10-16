using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class LayerInteractionController : Clickable, ISelectable
{
    public GameObject ParentObj;
    [SerializeField] private Color SelectedColor;

    public ushort LayerID { get; private set; }
    [SerializeField] private TMP_InputField LayerInput;

    private InputAction DoubleClickAction;
    protected override void Start()
    {
        base.Start();

        DoubleClickAction = InputSystem.actions.FindAction("DoubleClick");
        DoubleClickAction.performed += OnDoubleClick;

        LayerInput.enabled = false;
    }

    void OnDestroy()
    {
        DoubleClickAction.Dispose();
    }

    public void SetSelected(bool isSelected)
    {
        this.gameObject.GetComponentInParent<Image>().color = isSelected ? SelectedColor : Color.white;

        LayerInput.enabled = isSelected;
    }

    public LayerInteractionController SetID(ushort id)
    {
        LayerID = id;
        return this;
    }

    public LayerInteractionController SetText(string text)
    {
        LayerInput.text = text;
        return this;
    }

    void OnDoubleClick(InputAction.CallbackContext context)
    {
        if (IsInsideCollider())
        {
            LayerInput.ActivateInputField();
        }
    }

    public void TextChanged()
    {
        try
        {
            MeshData meshData = MeshRegistry.instance.GetMeshData(LayerID);
            meshData.name = LayerInput.text;
            Debug.Log(meshData.ToString());
        }
        catch (Exception)
        {
            Debug.LogError("Couldn't change meshData name.");
        }
    }
}

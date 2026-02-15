using System;
using System.Threading;
using Assets.Scripts.States;
using Assets.Scripts.Utility.MVI;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class LayerController : Clickable, IView<MeshLayerStates, LayerStates>
{
    public GameObject ParentObj;
    [SerializeField] private Color SelectedColor;

    public Guid ID { get; private set; }
    [SerializeField] private TMP_InputField LayerInput;
    [SerializeField] private TMP_Text DrawOrderText;

    private InputAction DoubleClickAction;

    private IViewModel<MeshLayerStates, LayerStates> _viewModel;

    protected override void Start()
    {
        base.Start();
        LayerInput.enabled = false;
    }

    void OnEnable()
    {
        DoubleClickAction = InputSystem.actions.FindAction("DoubleClick");
        DoubleClickAction.performed += OnDoubleClick;

        ParentObj.GetComponent<Button>().onClick.AddListener(() =>
        {
            _viewModel?.Send(new SelectLayerIntent(ID));
        });
    }

    void OnDestroy()
    {
        DoubleClickAction.performed -= OnDoubleClick;
        _viewModel?.Unbind(this);
    }

    public void SetSelected(bool isSelected)
    {
        this.gameObject.GetComponentInParent<Image>().color = isSelected ? SelectedColor : Color.white;
        LayerInput.enabled = isSelected;
    }

    public LayerController SetID(Guid id)
    {
        ID = id;
        return this;
    }

    void OnDoubleClick(InputAction.CallbackContext context)
    {
        if (IsInsideCollider())
        {
            Debug.LogWarning("Double clicked on layer " + ID);
            LayerInput.ActivateInputField(); // FIXME: doesn't seem to properly work on android yet
        }
    }

    public void TextChanged()
    {
        Debug.Log("Text changed: " + LayerInput.text);
        _viewModel?.Send(new ChangeLayerNameIntent(ID, LayerInput.text));
    }

    public void Render(LayerStates state)
    {
        if (!state.Layers.TryGetValue(ID, out var layer)) return;

        SetSelected(layer.IsSelected);
        if (!LayerInput.isFocused && LayerInput.text != layer.Name)
            LayerInput.text = layer.Name;
        DrawOrderText.text = layer.DrawOrder.ToString();
    }

    public void SetViewModel(IViewModel<MeshLayerStates, LayerStates> viewModel)
    {
        _viewModel = viewModel;
        _viewModel?.Bind(this);
    }
}

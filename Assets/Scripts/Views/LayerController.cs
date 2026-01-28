using System;
using Assets.Scripts.States;
using Assets.Scripts.Utility.MVI;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class LayerController : Clickable, IView<LayerStates>
{
    public GameObject ParentObj;
    [SerializeField] private Color SelectedColor;

    public Guid ID { get; private set; }
    [SerializeField] private TMP_InputField LayerInput;

    private InputAction DoubleClickAction;

    private IViewModel<LayerStates> _viewModel;

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
            LayerInput.ActivateInputField();
        }
    }

    public void TextChanged()
    {
        _viewModel?.Send(new ChangeLayerNameIntent(ID, LayerInput.text));
    }

    public void Render(LayerStates state)
    {
        if (state.Layers.TryGetValue(ID, out var layer))
        {
            SetSelected(layer.IsSelected);
            LayerInput.text = layer.Name;
        }
        else
        {
            Debug.Log("Couldn't fetch layer state");
        }
    }

    public void SetViewModel(IViewModel<LayerStates> viewModel)
    {
        _viewModel = viewModel;
        _viewModel?.Bind(this);
    }
}

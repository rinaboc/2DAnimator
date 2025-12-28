using System;
using Assets.Scripts.Utility.MVI;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

public class PopupWindowController : BaseUIController, IView<ParameterStates>
{
    private Button submitButton;

    [SerializeField, CreateProperty] private float m_minValue, m_maxValue, m_defaultValue;
    [SerializeField, CreateProperty] private string m_paramName;
    [SerializeField, CreateProperty] private string m_errorMessage = "";

    enum PopupMode
    {
        Create, Edit
    }
    private PopupMode m_popupMode = PopupMode.Create;

    private Guid _editedParamID;

    private IViewModel<ParameterStates> _viewModel;

    void OnEnable()
    {
        submitButton = ui.Q<Button>("CreateButton");
        submitButton.clicked += OnSubmitButtonClicked;

        ui.Q<Button>("ExitButton").clicked += () => _viewModel.Send(new CloseParameterSettingsIntent());
        Debug.Log($"{m_minValue}, {m_maxValue}, {m_defaultValue}");
    }

    public void OnParameterAddButtonClicked()
    {
        _viewModel.Send(new OpenParameterCreatorIntent());
    }

    public void EditParameter(ParameterState state)
    {
        _editedParamID = state.ID;
        m_popupMode = PopupMode.Edit;
        m_minValue = state.MinValue;
        m_maxValue = state.MaxValue;
        m_defaultValue = state.DefaultValue;
        m_paramName = state.Name;

        submitButton.text = "Save";

    }

    private void OnSubmitButtonClicked()
    {
        if (!ValidateInput()) return;

        if (m_popupMode == PopupMode.Create)
            _viewModel.Send(new CreateParameterIntent(Guid.NewGuid(), m_minValue, m_maxValue, m_defaultValue, m_paramName));
        else
            _viewModel.Send(new UpdateParameterIntent(_editedParamID, m_minValue, m_maxValue, m_defaultValue, m_paramName));

        _viewModel.Send(new CloseParameterSettingsIntent());
    }

    private bool ValidateInput()
    {
        if (m_minValue <= m_defaultValue && m_maxValue >= m_defaultValue && m_paramName != "")
        {
            HideErrorMessage();
            return true;
        }

        DisplayErrorMessage("Invalid values.");

        return false;
    }

    private void DisplayErrorMessage(string message)
    {
        m_errorMessage = message;
        ui.Q<Label>("ErrorMessage").visible = true;
    }

    private void HideErrorMessage()
    {
        ui.Q<Label>("ErrorMessage").visible = false;
    }

    public void Render(ParameterStates state)
    {
        if (state.IsSettingsOpen)
        {
            if (state.SelectedParamID != Guid.Empty)
                EditParameter(state.Parameters[state.SelectedParamID]);
            else
            {
                m_popupMode = PopupMode.Create;
                submitButton.text = "Create";
            }
        }

        ShowPanel(state.IsSettingsOpen);
    }

    public void SetViewModel(IViewModel<ParameterStates> viewModel)
    {
        _viewModel = viewModel;
        _viewModel.Bind(this);
    }
}

using System;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

public class PopupWindowController : BaseUIController
{
    private Button createButton;

    [SerializeField, CreateProperty] private float m_minValue, m_maxValue, m_defaultValue;
    [SerializeField, CreateProperty] private string m_paramName;
    [SerializeField, CreateProperty] private string m_errorMessage = "";

    enum PopupMode
    {
        Create, Edit
    }
    private PopupMode m_popupMode = PopupMode.Create;

    private Guid _editedParamID;

    void OnEnable()
    {
        UIEvents.EditParameterInfoEvent += EditParameter;
        createButton = ui.Q<Button>("CreateButton");
        ui.Q<Button>("ExitButton").clicked += () => ShowPanel(false);

        ShowPanel(false);

        Debug.Log($"{m_minValue}, {m_maxValue}, {m_defaultValue}");
    }

    void OnDisable()
    {
        ShowPanel(false);
        UIEvents.EditParameterInfoEvent -= EditParameter;
    }

    public void EditParameter(Guid paramID)
    {
        if (ParameterRegistry.Instance.TryGet(paramID, out Parameter parameter))
        {
            m_popupMode = PopupMode.Edit;
            ShowPanel(true);
            _editedParamID = parameter.ID;
            m_minValue = parameter.MinValue;
            m_maxValue = parameter.MaxValue;
            m_defaultValue = parameter.DefaultValue;
            m_paramName = parameter.Name;

            createButton.text = "Save";
        }
    }

    public void CreateParameter()
    {
        m_popupMode = PopupMode.Create;
        createButton.text = "Create";
        ShowPanel(true);
    }

    private void OnCreateButtonClicked()
    {
        if (!ValidateInput()) return;

        ParameterManager.Instance.CreateParameter(m_minValue, m_maxValue, m_defaultValue, m_paramName);
        ShowPanel(false);
    }

    private void OnEditButtonClicked()
    {
        if (!ValidateInput()) return;

        if (ParameterRegistry.Instance.TryGet(_editedParamID, out Parameter parameter))
        {
            parameter.MinValue = m_minValue;
            parameter.MaxValue = m_maxValue;
            parameter.DefaultValue = m_defaultValue;
            parameter.Name = m_paramName;

            ParameterManager.Instance.UpdateParameter(parameter);
        }
        else
        {
            Debug.LogError("Couldn't fetch parameter.");
        }

        ShowPanel(false);
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

    protected override void RemoveListeners()
    {
        createButton.clicked -= OnCreateButtonClicked;
        createButton.clicked -= OnEditButtonClicked;
    }

    protected override void AddListeners()
    {
        createButton.clicked += m_popupMode == PopupMode.Create ?
            OnCreateButtonClicked : OnEditButtonClicked;
    }
}

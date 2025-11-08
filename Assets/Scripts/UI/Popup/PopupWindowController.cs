using System;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

public class PopupWindowController : BaseUIController
{
    private Button createButton;

    [SerializeField, CreateProperty]
    private float m_minValue, m_maxValue, m_defaultValue;
    [SerializeField, CreateProperty]
    private string m_paramName;
    [SerializeField, CreateProperty]
    private string m_errorMessage = "";

    private Guid _editedParamID;

    void OnEnable()
    {
        UIEvents.EditParameterInfoEvent += EditParameter;
        createButton = ui.Q<Button>("CreateButton");

        ShowPanel(false);

        Debug.Log($"{m_minValue}, {m_maxValue}, {m_defaultValue}");
    }

    void OnDisable()
    {
        RemoveButtonListeners();
        UIEvents.EditParameterInfoEvent -= EditParameter;
    }

    private void RemoveButtonListeners()
    {
        createButton.clicked -= OnCreateButtonClicked;
        createButton.clicked -= OnEditButtonClicked;
    }

    public void EditParameter(Guid paramID)
    {
        if (ParameterRegistry.Instance.TryGet(paramID, out Parameter parameter))
        {
            ShowPanel(true);
            RemoveButtonListeners();
            _editedParamID = parameter.ID;
            m_minValue = parameter.MinValue;
            m_maxValue = parameter.MaxValue;
            m_defaultValue = parameter.DefaultValue;
            m_paramName = parameter.Name;

            createButton.text = "Save";
            createButton.clicked += OnEditButtonClicked;
        }
    }

    public void CreateParameter()
    {
        ShowPanel(true);
        RemoveButtonListeners();

        createButton.text = "Create";
        createButton.clicked += OnCreateButtonClicked;
        Debug.Log("Creating new parameter");
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
}

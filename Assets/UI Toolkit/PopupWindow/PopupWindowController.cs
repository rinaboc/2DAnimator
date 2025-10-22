using System;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class PopupWindowController : MonoBehaviour
{
    private VisualElement ui;
    private Button createButton;

    [SerializeField, CreateProperty]
    private float m_minValue, m_maxValue, m_defaultValue;
    [SerializeField, CreateProperty]
    private string m_paramName;
    [SerializeField, CreateProperty]
    private string m_errorMessage = "";

    private ushort _editedParamID;

    void Awake()
    {
        ui = GetComponent<UIDocument>().rootVisualElement;
        ui.dataSource = this;
    }

    void OnEnable()
    {
        createButton = ui.Q<Button>("CreateButton");

        HidePanel();

        Debug.Log($"{m_minValue}, {m_maxValue}, {m_defaultValue}");
    }

    void OnDisable()
    {
        RemoveButtonListeners();
    }

    private void RemoveButtonListeners()
    {
        createButton.clicked -= OnCreateButtonClicked;
        createButton.clicked -= OnEditButtonClicked;
    }

    public void EditParameter(Parameter parameter)
    {
        ShowPanel();
        RemoveButtonListeners();
        _editedParamID = parameter.ID;
        m_minValue = parameter.MinValue;
        m_maxValue = parameter.MaxValue;
        m_defaultValue = parameter.DefaultValue;
        m_paramName = parameter.Name;

        createButton.text = "Save";
        createButton.clicked += OnEditButtonClicked;
    }

    public void CreateParameter()
    {
        ShowPanel();
        RemoveButtonListeners();

        createButton.text = "Create";
        createButton.clicked += OnCreateButtonClicked;
        Debug.Log("Creating new parameter");
    }

    private void OnCreateButtonClicked()
    {
        if (!ValidateInput()) return;

        ParameterManager.instance.CreateParameter(m_minValue, m_maxValue, m_defaultValue, m_paramName);
        HidePanel();
    }

    private void OnEditButtonClicked()
    {
        if (!ValidateInput()) return;

        try
        {
            Parameter parameter = ParameterRegistry.Instance.GetParameter(_editedParamID);

            parameter.MinValue = m_minValue;
            parameter.MaxValue = m_maxValue;
            parameter.DefaultValue = m_defaultValue;
            parameter.Name = m_paramName;

            ParameterManager.instance.UpdateParameter(parameter);
        }
        catch (Exception)
        {
            Debug.LogError("Couldn't fetch parameter.");
        }

        HidePanel();
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

    private void SetVisible(string elementID, bool isVisible)
    {
        if (isVisible)
            ui.Q<VisualElement>(elementID).RemoveFromClassList("hide");
        else
            ui.Q<VisualElement>(elementID).AddToClassList("hide");
    }

    private void ShowPanel()
    {
        SetVisible("MainPanel", true);
    }

    private void HidePanel()
    {
        SetVisible("MainPanel", false);
    }
}

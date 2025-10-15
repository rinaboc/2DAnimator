using System;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class PopupWindowController : MonoBehaviour
{
    private VisualElement ui;
    private Button createButton;
    private TextField paramNameField;
    private FloatField minField;
    private FloatField maxField;
    private FloatField defaultField;

    [SerializeField, CreateProperty]
    private float m_minValue, m_maxValue, m_defaultValue;
    [SerializeField, CreateProperty]
    private string m_paramName;

    void Awake()
    {
        ui = GetComponent<UIDocument>().rootVisualElement;
        ui.dataSource = this;
    }

    void OnEnable()
    {
        createButton = ui.Q<Button>("CreateButton");

        paramNameField = ui.Q<TextField>("ParamName");
        minField = ui.Q<FloatField>("MinField");
        maxField = ui.Q<FloatField>("MaxField");
        defaultField = ui.Q<FloatField>("DefaultField");

        FillFields();

        Debug.Log($"{m_minValue}, {m_maxValue}, {m_defaultValue}");
    }

    void OnDisable()
    {
        createButton.clicked -= OnCreateButtonClicked;
        createButton.clicked -= OnEditButtonClicked;
    }

    private void FillFields()
    {
        try
        {
            ushort paramID = ParameterManager.instance.SelectedParamID;
            Parameter parameter = ParameterRegistry.instance.GetParameter(paramID);

            m_minValue = parameter.minValue;
            m_maxValue = parameter.maxValue;
            m_defaultValue = parameter.defaultValue;

            createButton.clicked += OnEditButtonClicked;
        }
        catch (Exception)
        {
            m_minValue = -1;
            m_maxValue = 1;
            m_defaultValue = 0;
            m_paramName = "parameter";
            createButton.clicked += OnCreateButtonClicked;
            Debug.Log("Creating new parameter");
        }
    }

    private void OnCreateButtonClicked()
    {
        ParameterManager.instance.CreateParameter(m_minValue, m_maxValue, m_defaultValue);
        gameObject.SetActive(false);
    }

    private void OnEditButtonClicked()
    {
        ushort paramID = ParameterManager.instance.SelectedParamID;
        Parameter parameter = ParameterRegistry.instance.GetParameter(paramID);

        parameter.minValue = m_minValue;
        parameter.maxValue = m_maxValue;
        parameter.defaultValue = m_defaultValue;

        Debug.Log("editing");
        gameObject.SetActive(false);
    }
}

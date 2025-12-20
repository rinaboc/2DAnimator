using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

[UxmlElement]
public partial class DialogElement : VisualElement
{
    [SerializeField, CreateProperty] private string m_dialogTitle = "Dialog";
    protected string DialogTitle { get => m_dialogTitle; set => m_dialogTitle = value; }
    protected VisualElement ContentContainer;

    public DialogElement()
    {
        AddToClassList("dialog");

        VisualElement header = new();
        header.AddToClassList("dialog-header");
        Add(header);

        Label dialogTitle = new();
        dialogTitle.SetBinding("text", new DataBinding
        {
            dataSource = this,
            dataSourcePath = new PropertyPath(nameof(m_dialogTitle)),
            bindingMode = BindingMode.ToTarget
        });
        dialogTitle.AddToClassList("dialog-title");
        header.Add(dialogTitle);

        Button exitButton = new();
        exitButton.name = "ExitButton";
        exitButton.AddToClassList("exit-button");
        header.Add(exitButton);

        ContentContainer = new();
        ContentContainer.AddToClassList("dialog-content");
        Add(ContentContainer);
    }

    public DialogElement(string title) : this()
    {
        DialogTitle = title;
    }
}

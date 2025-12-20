using UnityEngine.UIElements;

[UxmlElement]
public partial class TimelineSettingsElement : DialogElement
{
    public TimelineSettingsElement() : base("Timeline Settings")
    {
        style.width = 500;
        style.height = 220;
        style.top = 300;
        style.left = 1000;

        CreatePropertyLine("Max Frames", "MaxFrames");
        CreatePropertyLine("Frame Per Second", "FramePerSec");
    }

    private void CreatePropertyLine(string label, string propertyPath)
    {
        VisualElement container = new();
        container.AddToClassList("property-line");
        ContentContainer.Add(container);

        Label propertyLabel = new(label);
        propertyLabel.AddToClassList("property-label");
        container.Add(propertyLabel);

        IntegerField propertyField = new()
        {
            name = propertyPath
        };
        container.Add(propertyField);
    }
}
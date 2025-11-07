using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public abstract class BaseUIController : MonoBehaviour
{
    protected VisualElement ui;

    protected virtual void Awake()
    {
        ui = GetComponent<UIDocument>().rootVisualElement;
        ui.dataSource = this;
    }

    protected void ShowPanel(bool isVisible)
    {
        if (isVisible) ui.RemoveFromClassList("hide");
        else ui.AddToClassList("hide");

    }
}

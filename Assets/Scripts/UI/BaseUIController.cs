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
        if (isVisible)
        {
            ui.RemoveFromClassList("hide");
            AddListeners();
        }
        else
        {
            ui.AddToClassList("hide");
            RemoveListeners();
        }

    }

    protected virtual void RemoveListeners() { }
    protected virtual void AddListeners() { }
}

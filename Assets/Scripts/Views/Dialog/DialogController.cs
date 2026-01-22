using UnityEngine;
using UnityEngine.UIElements;

public class DialogController<T> : BaseUIController where T : DialogElement
{
    protected T dialogElement;
    override protected void Awake()
    {
        base.Awake();
        dialogElement = ui.Q<T>();
    }

    protected override void ShowPanel(bool isVisible)
    {
        if (isVisible)
        {
            dialogElement.RemoveFromClassList("hide");
            AddListeners();
        }
        else
        {
            dialogElement.AddToClassList("hide");
            RemoveListeners();
        }

    }
}

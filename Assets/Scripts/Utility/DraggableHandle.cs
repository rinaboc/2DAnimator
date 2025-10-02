using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))]
abstract public class DraggableHandle : Clickable
{
    [SerializeField] protected Transform ParentTransform;

    protected InputAction clickAndDragAction;
    protected bool dragging = false;

    protected Vector3 ClickWorldPosition
    {
        get
        {
            Vector3 clickWorldPos = cam.ScreenToWorldPoint(pointPositionAction.ReadValue<Vector2>());
            clickWorldPos.z = ParentTransform.position.z;
            return clickWorldPos;
        }
    }

    protected override void Start()
    {
        base.Start();

        clickAndDragAction = InputSystem.actions.FindAction("ClickAndDrag");
        clickAndDragAction.started += OnClickStarted;
        clickAndDragAction.performed += OnDragFinished;
    }

    protected void OnDestroy()
    {
        clickAndDragAction.started -= OnClickStarted;
        clickAndDragAction.performed -= OnDragFinished;
    }

    /// <summary>
    /// Fires when user taps anywhere on the screen. 
    /// </summary>
    /// <param name="context"></param>
    protected abstract void OnClickStarted(InputAction.CallbackContext context);
    protected abstract void OnDragFinished(InputAction.CallbackContext context);
}

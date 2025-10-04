using UnityEngine;
using UnityEngine.InputSystem;

public class MovementHandle : DraggableHandle
{
    [SerializeField] private Transform ArtObjectTransform;
    private Vector3 offset;

    void Update()
    {
        if (dragging)
        {
            Vector3 mouseWorldPos = cam.ScreenToWorldPoint(pointPositionAction.ReadValue<Vector2>());
            mouseWorldPos.z = ArtObjectTransform.position.z;
            ArtObjectTransform.position = mouseWorldPos + offset;
        }
    }

    protected override void OnClickStarted(InputAction.CallbackContext context)
    {
        if (IsInsideCollider())
        {
            Vector3 mouseWorldPos = cam.ScreenToWorldPoint(pointPositionAction.ReadValue<Vector2>());
            mouseWorldPos.z = ArtObjectTransform.position.z;
            offset = ArtObjectTransform.position - mouseWorldPos;
            dragging = true;
        }
    }

    protected override void OnDragFinished(InputAction.CallbackContext context)
    {
        if (dragging)
        {
            ArtObjectTransform.gameObject.GetComponent<ArtMesh>().UpdatePosition(ArtObjectTransform.localPosition);
        }
        dragging = false;
    }
}

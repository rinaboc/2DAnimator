using UnityEngine;
using UnityEngine.InputSystem;

public class MovementHandle : DraggableHandle
{
    private Vector3 offsetLocal;

    void Update()
    {
        if (dragging)
        {
            // ParentTransform.gameObject.GetComponent<ArtMesh>().MoveArtMesh(ClickWorldPosition + offset);
            Vector3 localClickPosition = ParentTransform.parent.InverseTransformPoint(ClickWorldPosition);

            ParentTransform.gameObject
                .GetComponent<ArtMesh>()
                .UpdateTransform(localClickPosition + offsetLocal, TransformType.POSITION);
        }
    }

    protected override void OnClickStarted(InputAction.CallbackContext context)
    {
        if (IsInsideCollider())
        {
            Vector3 localClickPosition = ParentTransform.parent.InverseTransformPoint(ClickWorldPosition);
            offsetLocal = ParentTransform.localPosition - localClickPosition;

            dragging = true;
        }
    }

    protected override void OnDragFinished(InputAction.CallbackContext context)
    {
        if (dragging)
        {
            ParentTransform.gameObject.GetComponent<ArtMesh>().SaveTransform(TransformType.POSITION);
        }
        dragging = false;
    }
}

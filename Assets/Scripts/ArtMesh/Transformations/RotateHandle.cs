using UnityEngine;
using UnityEngine.InputSystem;

public class RotateHandle : DraggableHandle
{
    private Quaternion defaultRotation = new();
    private Vector3 startingVec;

    protected override void OnClickStarted(InputAction.CallbackContext context)
    {
        if (IsInsideCollider())
        {
            startingVec = ClickWorldPosition - ParentTransform.position;
            defaultRotation = ParentTransform.localRotation;
            dragging = true;
            ParentTransform.gameObject.GetComponent<MeshController>().SendResetInterpolationIntent();
        }
    }

    protected override void OnDragFinished(InputAction.CallbackContext context)
    {
        if (dragging)
        {
            ParentTransform.gameObject.GetComponent<MeshController>().SaveTransform(TransformType.ROTATION);
        }
        dragging = false;
    }

    void Update()
    {
        if (dragging)
        {
            Vector3 rotatedVec = ClickWorldPosition - ParentTransform.position;

            Quaternion rotationQ = Quaternion.FromToRotation(startingVec, rotatedVec);
            ParentTransform.gameObject.GetComponent<MeshController>().UpdateTransform(defaultRotation * rotationQ, TransformType.ROTATION);
        }
    }
}

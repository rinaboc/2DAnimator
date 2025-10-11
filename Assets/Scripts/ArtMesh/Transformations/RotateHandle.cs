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
        }
    }

    protected override void OnDragFinished(InputAction.CallbackContext context)
    {
        if (dragging)
        {
            ArtMesh artMesh = ParentTransform.gameObject.GetComponent<ArtMesh>();
            artMesh.UpdateTransform(artMesh.gameObject.transform.localRotation, TransformType.ROTATION);
        }
        dragging = false;
    }

    void Update()
    {
        if (dragging)
        {
            Vector3 rotatedVec = ClickWorldPosition - ParentTransform.position;

            Quaternion rotationQ = Quaternion.FromToRotation(startingVec, rotatedVec);
            ParentTransform.gameObject.GetComponent<ArtMesh>().RotateArtMesh(defaultRotation, rotationQ);
        }
    }
}
